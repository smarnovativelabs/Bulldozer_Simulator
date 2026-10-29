import Foundation
import StoreKit

// Separate from Unity IAP's callback payload: only StoreKit's .verified branch
// may establish proof. This bridge never purchases, finishes, or grants content.
@available(iOS 15.0, macOS 12.0, *)
private enum QuarryAppleVerification {
    private final class Request {
        var state: Int32 = 0
        var released = false
        var task: Task<Void, Never>?
    }
    private static let lock = NSLock()
    private static var requests: [Int32: Request] = [:]
    private static var sequence: Int32 = 0
    private static let maximumRequests = 4

    static func begin(product: String, transaction: UInt64, nonConsumable: Bool) -> Int32 {
        lock.lock()
        defer { lock.unlock() }
        guard requests.count < maximumRequests else { return 0 }
        repeat { sequence = sequence == Int32.max ? 1 : sequence + 1 } while requests[sequence] != nil
        let id = sequence
        let request = Request()
        requests[id] = request
        request.task = Task {
            let accepted = await verify(product: product, transaction: transaction, nonConsumable: nonConsumable)
            complete(id, accepted: accepted)
        }
        return id
    }

    private static func complete(_ id: Int32, accepted: Bool) {
        lock.lock()
        defer { lock.unlock() }
        guard let request = requests[id] else { return }
        request.state = accepted ? 1 : -1
        request.task = nil
        if request.released { requests.removeValue(forKey: id) }
    }

    static func poll(_ id: Int32) -> Int32 {
        lock.lock()
        defer { lock.unlock() }
        return requests[id]?.state ?? -1
    }

    static func release(_ id: Int32) {
        lock.lock()
        defer { lock.unlock() }
        guard let request = requests[id] else { return }
        request.released = true
        request.task?.cancel()
        // A cancelled task retains its slot until StoreKit returns; cancellation
        // must not turn a stuck StoreKit call into an unbounded retry leak.
        if request.state != 0 { requests.removeValue(forKey: id) }
    }

    private static func matches(_ result: VerificationResult<StoreKit.Transaction>, product: String,
                                transaction: UInt64, nonConsumable: Bool) -> Bool {
        guard case .verified(let value) = result else { return false }
        return QuarryAppleTransactionMatches(
            actualID: value.id, expectedID: transaction, actualProduct: value.productID, expectedProduct: product,
            actualBundle: value.appBundleID, expectedBundle: Bundle.main.bundleIdentifier,
            quantity: value.purchasedQuantity, revoked: value.revocationDate != nil, upgraded: value.isUpgraded,
            correctType: value.productType == (nonConsumable ? .nonConsumable : .consumable))
    }

    private static func verify(product: String, transaction: UInt64, nonConsumable: Bool) async -> Bool {
        if let latest = await StoreKit.Transaction.latest(for: product),
           !Task.isCancelled, matches(latest, product: product, transaction: transaction, nonConsumable: nonConsumable) {
            return true
        }
        if Task.isCancelled { return false }
        // Recover older unfinished consumables too. Bound the scan; there is no
        // transaction-history sweep and no permanent transaction observer.
        var inspected = 0
        for await result in StoreKit.Transaction.unfinished {
            if Task.isCancelled { return false }
            if matches(result, product: product, transaction: transaction, nonConsumable: nonConsumable) { return true }
            inspected += 1
            if inspected >= 64 { break }
        }
        return false
    }
}

// Only called with StoreKit-verified fields above. Kept pure for boundary tests.
func QuarryAppleTransactionMatches(actualID: UInt64, expectedID: UInt64, actualProduct: String,
                                    expectedProduct: String, actualBundle: String, expectedBundle: String?,
                                    quantity: Int, revoked: Bool, upgraded: Bool, correctType: Bool) -> Bool {
    return expectedID != 0 && actualID == expectedID && !expectedProduct.isEmpty && actualProduct == expectedProduct &&
        expectedBundle != nil && !actualBundle.isEmpty && actualBundle == expectedBundle &&
        quantity == 1 && !revoked && !upgraded && correctType
}

@_cdecl("QuarryAppleVerificationSupported")
public func QuarryAppleVerificationSupported() -> Int32 {
    if #available(iOS 15.0, macOS 12.0, *) { return 1 }
    return 0
}

@_cdecl("QuarryAppleVerificationBegin")
public func QuarryAppleVerificationBegin(_ product: UnsafePointer<CChar>?, _ transaction: UnsafePointer<CChar>?,
                                         _ nonConsumable: Int32) -> Int32 {
    guard #available(iOS 15.0, macOS 12.0, *), let product = product, let transaction = transaction,
          let transactionID = UInt64(String(cString: transaction)), transactionID != 0 else { return 0 }
    let productID = String(cString: product)
    guard !productID.isEmpty && productID.utf8.count <= 256 else { return 0 }
    return QuarryAppleVerification.begin(product: productID, transaction: transactionID, nonConsumable: nonConsumable != 0)
}

@_cdecl("QuarryAppleVerificationPoll")
public func QuarryAppleVerificationPoll(_ request: Int32) -> Int32 {
    if #available(iOS 15.0, macOS 12.0, *) { return QuarryAppleVerification.poll(request) }
    return -1
}

@_cdecl("QuarryAppleVerificationRelease")
public func QuarryAppleVerificationRelease(_ request: Int32) {
    if #available(iOS 15.0, macOS 12.0, *) { QuarryAppleVerification.release(request) }
}


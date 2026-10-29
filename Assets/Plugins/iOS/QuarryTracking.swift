import AppTrackingTransparency
import Foundation

@_cdecl("QuarryTrackingStatus")
public func QuarryTrackingStatus() -> Int32 {
    if #available(iOS 14, *) { return Int32(ATTrackingManager.trackingAuthorizationStatus.rawValue) }
    return 2
}
@_cdecl("QuarryRequestTracking")
public func QuarryRequestTracking() {
    DispatchQueue.main.async {
        if #available(iOS 14, *), ATTrackingManager.trackingAuthorizationStatus == .notDetermined {
            ATTrackingManager.requestTrackingAuthorization { _ in }
        }
    }
}

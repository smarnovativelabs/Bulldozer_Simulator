using UnityEngine;
using UnityEngine.InputSystem;

namespace Bulldozer.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class MinerController : MonoBehaviour
    {
        public Transform visual;
        public MinerCamera followCamera;
        public MinerTouchControls touchControls;
        [Min(.1f)] public float walkSpeed = 3.2f;
        [Min(.1f)] public float runSpeed = 5.5f;
        public float acceleration = 14f;
        public float jumpHeight = 1.05f;
        public float gravity = -24f;
        public Vector2 MoveInput { get; private set; }
        public float Speed { get; private set; }
        public float CartSignedSpeed { get; private set; }
        public bool Grounded => motor && motor.isGrounded;
        public float Swing { get; private set; }
        public bool IsMining => swingTime > 0;
        public float VerticalSpeed => vertical;
        public bool MineWall;
        public float miningDuration = .967f;
        // Playback multiplier for the authored strike clip; its contact events scale with it.
        [Min(.5f)] public float swingSpeed = 1.6f;
        public float SwingDuration => miningDuration / Mathf.Max(.5f, swingSpeed);
        // Brief freeze of the strike on contact so each hit lands with weight. Only the
        // miner's swing pauses; world time, physics and UI keep running.
        float hitStopUntil;
        public bool InHitStop => Time.time < hitStopUntil;
        public void HitStop(float seconds) { hitStopUntil = Mathf.Max(hitStopUntil, Time.time + seconds); }
        public bool autoMine=true;
        public bool analogRun=true;
        public void CancelMining() { swingTime=0; Swing=0; mineRequested=false; }
        public bool ExcavationInputBlocked {get;set;}
        public bool GuidanceInputBlocked {get;set;}
        public bool IntroInputBlocked {get;set;}
        public bool InputBlocked => suppressUntilRelease || Bulldozer.Services.QuarryServiceOverlay.BlocksInput || IntroInputBlocked || GuidanceInputBlocked || ExcavationInputBlocked || Bulldozer.UI.QuarryGameUI.BlocksInput || (Bulldozer.UI.CommonUI.Instance != null && Bulldozer.UI.CommonUI.Instance.BlocksGameplayInput);
        bool suppressUntilRelease;
        int releaseFrame;
        public void SuppressInputUntilRelease()
        {
            suppressUntilRelease=true;releaseFrame=Time.frameCount;
            jumpRequested=mineRequested=false;CancelMining();
            if(touchControls){touchControls.Move=Vector2.zero;touchControls.Sprinting=false;touchControls.ConsumeLook();}
        }
        bool ActionHeld()
        {
            if(Mouse.current!=null&&Mouse.current.leftButton.isPressed)return true;
            if(Keyboard.current!=null&&(Keyboard.current.spaceKey.isPressed||Keyboard.current.enterKey.isPressed||Keyboard.current.escapeKey.isPressed))return true;
            if(Gamepad.current!=null&&(Gamepad.current.buttonSouth.isPressed||Gamepad.current.buttonEast.isPressed))return true;
            if(Touchscreen.current!=null)foreach(var touch in Touchscreen.current.touches)if(touch.press.isPressed)return true;
            return false;
        }
        CharacterController motor;
        Vector3 velocity, spawn;
        Vector3 cartAcceleration;
        float vertical, swingTime, idleTime, nextSwing;
        bool jumpRequested, mineRequested;
        MinerMining mining;
        MinerEquipment movementEquipment;
        public QuarryWheelbarrow ActiveWheelbarrow { get; set; }
        public void Jump() { if(!InputBlocked)jumpRequested = true; }
        public void Mine() { if(!InputBlocked)mineRequested = true; }
        void Awake() { motor = GetComponent<CharacterController>(); spawn = transform.position; mining=visual ? visual.GetComponent<MinerMining>() : null;movementEquipment=visual?visual.GetComponent<MinerEquipment>():null; }
        void OnEnable() { velocity = cartAcceleration = Vector3.zero; vertical = 0; jumpRequested = mineRequested = false; }
        void Start() { SettleSpawn(); }
        void SettleSpawn()
        {
            // CharacterController.isGrounded is undefined until its first Move. Close only
            // the small authored spawn gap, before the Animator evaluates Airborne.
            Physics.SyncTransforms();
            if(Physics.Raycast(transform.position+Vector3.up*.08f,Vector3.down,out var hit,.30f,~0,QueryTriggerInteraction.Ignore))
            {
                motor.Move(Vector3.down*Mathf.Max(.02f,hit.distance));
                if(motor.isGrounded)vertical=-2;
            }
        }
        void Update()
        {
            if(suppressUntilRelease&&Time.frameCount>releaseFrame+1&&!ActionHeld())suppressUntilRelease=false;
            // A zero-distance CharacterController.Move during UI pause clears its
            // grounded flag and incorrectly sends the animator into the jump state.
            if(Time.deltaTime<=0){jumpRequested=mineRequested=false;MoveInput=Vector2.zero;Speed=CartSignedSpeed=0;return;}
            var keyboard = Keyboard.current;
            var pad = Gamepad.current;
            Vector2 input = touchControls ? touchControls.Move : Vector2.zero;
            bool run = touchControls && touchControls.Sprinting;
            if (keyboard != null)
            {
                input += new Vector2((keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1 : 0),
                    (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1 : 0) - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1 : 0));
                run |= keyboard.leftShiftKey.isPressed;
                jumpRequested |= keyboard.spaceKey.wasPressedThisFrame;
                mineRequested |= keyboard.eKey.wasPressedThisFrame;
            }
            if (pad != null) { input += pad.leftStick.ReadValue(); run |= pad.leftStickButton.isPressed; jumpRequested |= pad.buttonSouth.wasPressedThisFrame; mineRequested |= pad.buttonWest.wasPressedThisFrame; }
            if (InputBlocked) { CancelMining(); input = Vector2.zero; run = false; jumpRequested = mineRequested = false; }
            input=Vector2.ClampMagnitude(input,1);
            if(input.magnitude<.12f)input=Vector2.zero;
            if(IsMining && (!Grounded||input.sqrMagnitude>.0225f||jumpRequested)) CancelMining();
            if(IsMining) { input=Vector2.zero; jumpRequested=false; }
            MoveInput = Vector2.ClampMagnitude(input, 1);
            float yaw = followCamera ? followCamera.Yaw : 0;
            Vector3 direction = Quaternion.Euler(0, yaw, 0) * new Vector3(MoveInput.x, 0, MoveInput.y);
            float movementMultiplier=movementEquipment?movementEquipment.MovementMultiplier:1f;
            bool pushing = ActiveWheelbarrow && ActiveWheelbarrow.Pushing;
            Quaternion facing = visual ? visual.rotation : transform.rotation;
            bool pulling = pushing && direction.sqrMagnitude > .01f && Vector3.Dot(facing*Vector3.forward,direction.normalized)<-.35f;
            Vector3 desiredVelocity = direction * ((run||analogRun ? runSpeed : walkSpeed)*movementMultiplier);
            if(pulling)desiredVelocity*=.65f;
            if (pushing)
            {
                // Ease into motion, but brake and reverse promptly when the input changes.
                bool braking = direction.sqrMagnitude < .01f || Vector3.Dot(velocity, desiredVelocity) < 0;
                velocity = Vector3.SmoothDamp(velocity, desiredVelocity, ref cartAcceleration,
                    braking ? ActiveWheelbarrow.brakingTime : ActiveWheelbarrow.accelerationTime,
                    Mathf.Infinity, Time.deltaTime);
                if (direction == Vector3.zero && velocity.sqrMagnitude < .0001f)
                    velocity = cartAcceleration = Vector3.zero;
            }
            else
            {
                cartAcceleration = Vector3.zero;
                velocity = Vector3.MoveTowards(velocity, desiredVelocity, acceleration * Time.deltaTime);
            }
            // Pull away without first requiring room for a 180-degree cart turn.
            if (pushing) facing = ActiveWheelbarrow.SmoothFacing(pulling?-direction:direction, facing, Time.deltaTime);
            else if (direction.sqrMagnitude > .01f && visual) facing = Quaternion.Slerp(facing, Quaternion.LookRotation(direction), 1 - Mathf.Exp(-14 * Time.deltaTime));
            if(pushing)
            {
                // Push along the handles, never strafe sideways while the cart is turning.
                Vector3 forward=facing*Vector3.forward;
                // Preserve the sign while braking too; releasing reverse must not kick forward.
                velocity=forward*Vector3.Dot(velocity,forward);
            }
            if(InputBlocked)velocity=cartAcceleration=Vector3.zero;
            Vector3 movement = velocity * Time.deltaTime;
            if (pushing)
            {
                Vector3 requestedMovement = movement;
                ActiveWheelbarrow.ConstrainMovement(ref movement, ref facing);
                // Do not retain smoothing momentum into a wall after the sweep has stopped us.
                if ((movement-requestedMovement).sqrMagnitude > .00000001f) cartAcceleration = Vector3.zero;
                velocity = Time.deltaTime > 0 ? movement / Time.deltaTime : Vector3.zero;
                jumpRequested = false;
            }
            if (visual) visual.rotation = facing;
            if (motor.isGrounded && vertical < 0) vertical = -2;
            if (jumpRequested && motor.isGrounded) vertical = Mathf.Sqrt(jumpHeight * -2 * gravity);
            vertical = Mathf.Max(vertical + gravity * Time.deltaTime, -35);
            var flags = motor.Move(movement + Vector3.up * vertical * Time.deltaTime);
            if (pushing && (flags & CollisionFlags.Sides) != 0)
            {
                velocity = new Vector3(motor.velocity.x, 0, motor.velocity.z);
                cartAcceleration = Vector3.zero;
            }
            if ((flags & CollisionFlags.Above) != 0 && vertical > 0) vertical = 0;
            Speed = new Vector2(motor.velocity.x, motor.velocity.z).magnitude;
            CartSignedSpeed=pushing?Vector3.Dot(motor.velocity,visual.forward):0;
            idleTime=MoveInput.sqrMagnitude<.01f?idleTime+Time.deltaTime:0;
            bool autoRequested=autoMine&&idleTime>.08f&&Time.time>=nextSwing&&Speed<.12f&&!InputBlocked;
            if ((mineRequested||autoRequested) && swingTime <= 0 && Grounded && visual && visual.TryGetComponent<MinerEquipment>(out var equipment) && equipment.PickaxeVisible)
            {
                nextSwing=Time.time+.1f;
                if(mining&&mining.TryBeginSwing()){swingTime=SwingDuration;nextSwing=Time.time+SwingDuration+.06f;velocity=Vector3.zero;}
            }
            // The swing clock waits during hit-stop so the clip and swing stay in step.
            if (!InHitStop) swingTime = Mathf.Max(0, swingTime - Time.deltaTime);
            Swing = swingTime > 0 ? Mathf.Sin((1 - swingTime / SwingDuration) * Mathf.PI) : 0;
            jumpRequested = mineRequested = false;
            if (transform.position.y < -20) { motor.enabled = false; transform.position = spawn; motor.enabled = true; vertical = 0; velocity = Vector3.zero; SettleSpawn(); }
        }
    }
}






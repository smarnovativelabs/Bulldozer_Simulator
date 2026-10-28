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
        public bool Grounded => motor && motor.isGrounded;
        public float Swing { get; private set; }
        public bool IsMining => swingTime > 0;
        public float VerticalSpeed => vertical;
        public bool MineWall;
        public float miningDuration = .967f;
        public bool autoMine=true;
        public bool analogRun=true;
        public void CancelMining() { swingTime=0; Swing=0; mineRequested=false; }
        public bool InputBlocked => Bulldozer.UI.CommonUI.Instance != null && Bulldozer.UI.CommonUI.Instance.BlocksGameplayInput;
        CharacterController motor;
        Vector3 velocity, spawn;
        float vertical, swingTime, idleTime, nextSwing;
        bool jumpRequested, mineRequested;
        MinerMining mining;
        public void Jump() => jumpRequested = true;
        public void Mine() => mineRequested = true;
        void Awake() { motor = GetComponent<CharacterController>(); spawn = transform.position; mining=visual ? visual.GetComponent<MinerMining>() : null; }
        void OnEnable() { velocity = Vector3.zero; vertical = 0; jumpRequested = mineRequested = false; }
        void Update()
        {
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
            velocity = Vector3.MoveTowards(velocity, direction * (run||analogRun ? runSpeed : walkSpeed), acceleration * Time.deltaTime);
            if (direction.sqrMagnitude > .01f && visual) visual.rotation = Quaternion.Slerp(visual.rotation, Quaternion.LookRotation(direction), 1 - Mathf.Exp(-14 * Time.deltaTime));
            if (motor.isGrounded && vertical < 0) vertical = -2;
            if (jumpRequested && motor.isGrounded) vertical = Mathf.Sqrt(jumpHeight * -2 * gravity);
            vertical = Mathf.Max(vertical + gravity * Time.deltaTime, -35);
            var flags = motor.Move((velocity + Vector3.up * vertical) * Time.deltaTime);
            if ((flags & CollisionFlags.Above) != 0 && vertical > 0) vertical = 0;
            Speed = new Vector2(motor.velocity.x, motor.velocity.z).magnitude;
            idleTime=MoveInput.sqrMagnitude<.01f?idleTime+Time.deltaTime:0;
            bool autoRequested=autoMine&&idleTime>.16f&&Time.time>=nextSwing&&Speed<.12f&&!InputBlocked;
            if ((mineRequested||autoRequested) && swingTime <= 0 && Grounded && visual && visual.TryGetComponent<MinerEquipment>(out var equipment) && equipment.PickaxeVisible)
            {
                nextSwing=Time.time+.16f;
                if(mining&&mining.TryBeginSwing()){swingTime=miningDuration;nextSwing=Time.time+miningDuration+.22f;velocity=Vector3.zero;}
            }
            swingTime = Mathf.Max(0, swingTime - Time.deltaTime);
            Swing = swingTime > 0 ? Mathf.Sin((1 - swingTime / miningDuration) * Mathf.PI) : 0;
            jumpRequested = mineRequested = false;
            if (transform.position.y < -20) { motor.enabled = false; transform.position = spawn; motor.enabled = true; vertical = 0; velocity = Vector3.zero; }
        }
    }
}





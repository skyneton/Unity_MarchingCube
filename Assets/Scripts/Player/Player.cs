using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    public ChunkCoord PlayerChunkCoord;
    public World world;

    public float mouseSensitivity = 4.5f;
    [SerializeField]
    private float pitchLimit = 90f; // Player Rotation Y Limit
    [SerializeField]
    private float walkSpeed = 4.5f;
    [SerializeField]
    private float jumpPower = 6f;

    public bool IsGround { get; private set; }

    [SerializeField] private Collider collid = null;

    [SerializeField]
    private Rigidbody rigid = null;

    public Text text;

    internal Vector3 lastPosition;
    
    public static bool IsMobile
    {
        get => Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer;
    }

    private void Start()
    {
        CursorManager.CursorLock();
        CheckChunkLoaded();
        
        text.text = transform.position.ToString();
    }

    private void Update()
    {
        CheckChunkLoaded();
        
        text.text = transform.position.ToString();
        
        if(!IsMobile) CursorCheck();
        if (!IsMobile && !CursorManager.IsCursorLock) return;
        
        IsGroundCheck();
        PlayerRotationUpdate();
        if (rigid.IsSleeping()) return;
        Jump();
    }

    private void FixedUpdate()
    {
        if (!IsMobile && !CursorManager.IsCursorLock) return;
        if (rigid.IsSleeping()) return;
        Move();
    }

    private void IsGroundCheck()
    {
        IsGround = Physics.Raycast(transform.position, Vector3.down, collid.bounds.extents.y + .35f) && Mathf.Abs(rigid.velocity.y) < 1.5f;
    }

    private void CursorCheck()
    {
        if(!CursorManager.IsFocus && Cursor.lockState == CursorLockMode.Locked)
            CursorManager.CursorUnLock();
        else if (Input.GetKeyUp(KeyCode.Escape))
        {
            if(CursorManager.IsFocus) CursorManager.CursorLock();
            else CursorManager.CursorUnLock();
        }
    }

    private void PlayerRotationUpdate()
    {
        if (!IsMobile)
        {
            float mouseVertical = Input.GetAxis("Mouse Y");
            Vector3 cameraRotation = Camera.main.transform.localEulerAngles;
            if (cameraRotation.x > 180f) cameraRotation.x -= 360f;
            cameraRotation.x =
                Mathf.Clamp(cameraRotation.x - mouseVertical * mouseSensitivity, -pitchLimit, pitchLimit);
            Camera.main.transform.localEulerAngles = cameraRotation;

            float mouseHorizontal = Input.GetAxis("Mouse X");
            transform.Rotate(Vector3.up, mouseHorizontal * mouseSensitivity);
        }
    }

    private void Jump()
    {
        if (IsGround && Input.GetKey(KeyManager.Jump))
        {
            Vector3 velocity = rigid.velocity;
            velocity.y = jumpPower;
            rigid.velocity = velocity;
            
            IsGround = false;
        }
    }

    private void Move()
    {
        int vertical = 0, horizontal = 0;
        float plusSpeed = 1f;
        
        if (!IsMobile)
        {
            if(Input.GetKey(KeyManager.Forward))
                vertical += 1;
            if (Input.GetKey(KeyManager.Backward))
                vertical -= 1;

            if (Input.GetKey(KeyManager.Left))
                horizontal += 1;
            if (Input.GetKey(KeyManager.Right))
                horizontal -= 1;
            if (vertical == 1 && Input.GetKey(KeyManager.Run))
                plusSpeed += .2f;
        }
        
        transform.Translate((vertical * Vector3.forward + horizontal * Vector3.left).normalized * walkSpeed * plusSpeed * Time.fixedDeltaTime);
    }

    private void CheckChunkLoaded()
    {
        if (PlayerChunkCoord == null || !world.ChunkActivateInPos(transform.position))
        {
            rigid.Sleep();
            transform.position = lastPosition;
            
        }else if (rigid.IsSleeping())
        {
            rigid.WakeUp();
        }

        lastPosition = transform.position;
    }
}
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Components:")]
    [SerializeField] private Transform cam;
    [SerializeField] private World world;
    [SerializeField] private GameObject canvas;
    [SerializeField] private GameObject pauseUI;
    [SerializeField] private Transform hightlightblock;
    [SerializeField] private Transform placeBlock;
    [SerializeField] private AudioSource currentsfx;

    [Header("Variables:")]
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool isSprinting;
    [SerializeField] private bool isSnicking;
    [SerializeField] private bool canPoseBlock;
    [SerializeField] public bool isPause;
    [SerializeField] private bool isGlobalScreen;
    [SerializeField] private bool jumpRequest;

    [Range(0f, 10f)]
    [SerializeField] private float walkSpeed = 3f;
    [Range(0f, 10f)]
    [SerializeField] private float sprintSpeed = 6f;
    [Range(0f, 10f)]
    [SerializeField] private float snickSpeed = 1.5f;
    [Range(0f, 10f)]
    [SerializeField] private float jumpForce = 5f;
    [Range(-10f, 0f)]
    [SerializeField] private float gravity = -9.8f;
    [Range(-90f, 90f)]
    [SerializeField] private float maxRoation = 90f;
    [Range(-90, 90f)]
    [SerializeField] private float minRoation = -90f;
    [Range(0f, 10f)]
    [SerializeField] private float playerWidth = 0.15f;
    [Range(0f, 10f)]
    [SerializeField] private float boundsTolerance = 0.1f;
    [Range(-10, 10)]
    [SerializeField] private float reach = 5f;
    [Range(-10, 10)]
    [SerializeField] private float checkIncrement = 0.1f;
    [SerializeField] private byte selectedBlockIndex = 1;

    [SerializeField] private float horizontal;
    [SerializeField] private float vertical;
    [SerializeField] private float mouseHorizontal;
    [SerializeField] private float mouseVertical;
    [SerializeField] private Vector3 velocity;
    [SerializeField] private float verticalMomentum = 0;

    private void Start()
    {
        cam = GameObject.Find("Main Camera").transform;
        world = GameObject.Find("World").GetComponent<World>();
        hightlightblock = GameObject.Find("SelectedBlock").transform;
        placeBlock = GameObject.Find("UpperSelectedBlock").transform;
        currentsfx = GetComponent<AudioSource>();
        canvas = GameObject.Find("Canvas");
    }

    private void FixedUpdate()
    {

        CalculateVelocity();
        if (jumpRequest)
            Jump();

        transform.Rotate(Vector3.up * mouseHorizontal);
        cam.Rotate(Vector3.right * -Mathf.Clamp(mouseVertical, minRoation, maxRoation));
        transform.Translate(velocity, Space.World);

    }

    private void Update()
    {

        if (!isPause)
        {
            Cursor.visible = false;
            pauseUI.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            GetPlayerInputs();
            placeCursorBlocks();
        }
        else
        {
            Cursor.visible = true;
            pauseUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
        }

        canvas.SetActive(!isGlobalScreen);

        if(transform.position.y <= -30)
        {
            transform.position = world.spawnPoint;
        }

        getExternalPlayerInputs();
    }

    private void Jump()
    {

        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;

    }

    private void CalculateVelocity()
    {

        // Affect vertical momentum with gravity.
        if (verticalMomentum > gravity)
            verticalMomentum += Time.fixedDeltaTime * gravity;

        // if we're sprinting, use the sprint multiplier.
        if (isSprinting)
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * sprintSpeed;
        else if(!isSprinting && !isSnicking)
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;
        else if(isSnicking)
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * snickSpeed;

        // Apply vertical momentum (falling/jumping).
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
            velocity.z = 0;
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
            velocity.x = 0;

        if (velocity.y < 0)
            velocity.y = checkDownSpeed(velocity.y);
        else if (velocity.y > 0)
            velocity.y = checkUpSpeed(velocity.y);


    }

    private void GetPlayerInputs()
    {

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");

        if (Input.GetButtonDown("Sprint"))
            isSprinting = true;
        if (Input.GetButtonUp("Sprint"))
            isSprinting = false;
        if (Input.GetButtonUp("Fire3"))
        {
            isSnicking = false;
            cam.position = new Vector3(gameObject.transform.position.x, cam.position.y + 0.5f, gameObject.transform.position.z);
        }
        if (Input.GetButtonDown("Fire3")) {
            isSnicking = true;
            cam.position = new Vector3(gameObject.transform.position.x, cam.position.y - 0.5f, gameObject.transform.position.z);
        }

        if (isGrounded && Input.GetButtonDown("Jump")) {
            jumpRequest = true;
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            isGlobalScreen = !isGlobalScreen;
        }

        if (hightlightblock.gameObject.activeSelf)
        {

            if (Input.GetMouseButtonDown(0) && !world.checkForUnbreakableVoxel(hightlightblock.position))
            {
                world.getChunkFromVector3(hightlightblock.position).editVoxel(hightlightblock.position, 0);
                currentsfx.clip = world.getBlockTypeVoxel(hightlightblock.position).getPoseSFX;
                world.getBlockTypeVoxel(hightlightblock.position).onBreak.Invoke();
                currentsfx.Play();
            }
            else if(Input.GetMouseButtonDown(0) && !canPoseBlock)
            {
                world.getItemTypes[selectedBlockIndex].onRightClick.Invoke();
            }

            if (Input.GetMouseButtonDown(1) && canPoseBlock)
            {
                world.getChunkFromVector3(placeBlock.position).editVoxel(placeBlock.position, selectedBlockIndex);
                currentsfx.clip = world.getBlockTypes[selectedBlockIndex].getPoseSFX;
                world.getBlockTypeVoxel(hightlightblock.position).onPosed.Invoke();
                currentsfx.Play();
            }
            else if(Input.GetMouseButtonDown(1) && !canPoseBlock)
            {
                world.getItemTypes[selectedBlockIndex].onLeftClick.Invoke();
            }

        }
    }

    private void getExternalPlayerInputs() {
        if (Input.GetButtonDown("Cancel"))
        {
            isPause = !isPause;
        }else if (Input.GetKeyDown(KeyCode.F2))
        {
           StartCoroutine(captureScreen(Screen.width, Screen.height));
        }
    }

    private void placeCursorBlocks()
    {
        float step = checkIncrement;
        Vector3 lastPos = new Vector3();

        while (step < reach)
        {

            Vector3 pos = cam.position + (cam.forward * step);

            if (world.checkForVoxel(pos))
            {

                hightlightblock.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                hightlightblock.gameObject.SetActive(true);

                placeBlock.position = lastPos;
                placeBlock.gameObject.SetActive(true);

                return;
            }

            lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
            step += checkIncrement;

        }

        hightlightblock.gameObject.SetActive(false);
        placeBlock.gameObject.SetActive(false);
    }

    WaitForSeconds waitTime = new WaitForSeconds(0.1F);
    WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();
    private IEnumerator captureScreen(int width, int height){

        string dir = Application.persistentDataPath + "/Screenshots";


        if (!Directory.Exists(dir))
        {
            var folder = Directory.CreateDirectory(dir);
        }

        Camera camera = cam.GetComponent<Camera>();

        yield return waitTime;
        yield return frameEnd;

        camera.targetTexture = RenderTexture.GetTemporary(width, height, 16);
        RenderTexture renderTexture = camera.targetTexture;

        Texture2D renderResult = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
        Rect rect = new Rect(0, 0, renderTexture.width, renderTexture.height);
        renderResult.ReadPixels(rect, 0, 0);    

        byte[] byteArray = renderResult.EncodeToPNG();
        File.WriteAllBytes(dir + "/" + System.DateTime.Now.ToString("yyyy_MM_dd HH mm ss") + ".png", byteArray);
        Debug.Log("Saved Screenshot");
        camera.targetTexture = null;
    }

    private float checkDownSpeed(float downSpeed)
    {

        if (
            world.checkForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
            world.checkForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
            world.checkForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)) ||
            world.checkForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth))
           )
        {

            isGrounded = true;
            return 0;

        }
        else
        {

            isGrounded = false;
            return downSpeed;

        }

    }

    private float checkUpSpeed(float upSpeed)
    {

        if (
            world.checkForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth)) ||
            world.checkForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth))||
            world.checkForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth)) ||
            world.checkForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth))
           )
        {

            return 0;

        }
        else
        {

            return upSpeed;

        }

    }

    private bool front
    {

        get
        {
            if (
                world.checkForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z + playerWidth)) ||
                world.checkForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z + playerWidth))
                )
                return true;
            else
                return false;
        }

    }
    private bool back
    {

        get
        {
            if (
                world.checkForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z - playerWidth)) ||
                world.checkForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z - playerWidth))
                )
                return true;
            else
                return false;
        }

    }
    private bool left
    {

        get
        {
            if (
                world.checkForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y, transform.position.z)) ||
                world.checkForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 1f, transform.position.z))
                )
                return true;
            else
                return false;
        }

    }
    private bool right
    {

        get
        {
            if (
                world.checkForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y, transform.position.z)) ||
                world.checkForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 1f, transform.position.z))
                )
                return true;
            else
                return false;
        }

    }

    public Transform getSelectedBlock
    {
        get
        {
            return hightlightblock;
        }
    }

    public Transform getSelectedPlaceBlock
    {
        get
        {
            return placeBlock;
        }
    }

    public World getWorld
    {
        get
        {
            return world;
        }
    }


    public byte getSelectedBlockType
    {
        get
        {
            return selectedBlockIndex;
        }
        set
        {
            selectedBlockIndex = value;
        }
    }

    public bool getCanPoseBlock
    {
        get { return canPoseBlock; }
        set { canPoseBlock = value; }
    }
}

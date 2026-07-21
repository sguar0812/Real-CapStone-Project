using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class Laser : MonoBehaviour
{
    public GameObject DartPrefab;

    [Tooltip("Optional VR fire action. Leave empty if you don't use Input System actions.")]
    [SerializeField] private UnityEngine.InputSystem.InputActionReference fireAction;

    private XRBaseInteractable xrInteractable;

    //private Transform SpawnLocation;

    [SerializeField] private Transform SpawnLocation;


    // Code to test gun from laptop
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)
            || (IsHeldByInteractor() && (CheckControllerTrigger(leftDevice, ref leftTriggerPressed)
            || CheckControllerTrigger(rightDevice, ref rightTriggerPressed))))
        {
            ShootDart();
        }
    }

    private bool IsHeldByInteractor()
    {
        if (xrInteractable == null)
            return false;

        return xrInteractable.isSelected;
    }

    private bool leftTriggerPressed;
    private bool rightTriggerPressed;
    private InputDevice leftDevice;
    private InputDevice rightDevice;

    private bool CheckControllerTrigger(InputDevice device, ref bool wasPressed)
    {
        if (!device.isValid)
            return false;

        if (device.TryGetFeatureValue(CommonUsages.triggerButton, out bool isPressed))
        {
            if (isPressed && !wasPressed)
            {
                wasPressed = true;
                return true;
            }

            wasPressed = isPressed;
        }

        return false;
    }

    private GameObject SpawnDart()
    {

        if (SpawnLocation == null)
        {
            Debug.LogError($"{name}: SpawnLocation is not assigned. Cannot spawn dart.");
            return null;
        }

        Debug.Log($"Spawning from {SpawnLocation.position}");

        #region ...
        //check if prefab is null
        if (DartPrefab == null)
        {
            Debug.LogError($"{name}: DartPrefab is not assigned. Cannot spawn dart.");
            return null;
        }
        #endregion

        GameObject Dart = null;

        Debug.Log("LAUNCH THE PROJECTILE!");

        Dart = Instantiate(DartPrefab, SpawnLocation.position, SpawnLocation.rotation);

        return Dart;
    }
 private AudioSource audioSource;








    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        xrInteractable = GetComponent<XRBaseInteractable>();
    }

    private void OnEnable()
    {
        InitializeDevices();
        InputDevices.deviceConnected += OnDeviceConnected;
        InputDevices.deviceDisconnected += OnDeviceDisconnected;

        if (fireAction != null && fireAction.action != null)
        {
            fireAction.action.performed += OnFire;
            fireAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        InputDevices.deviceConnected -= OnDeviceConnected;
        InputDevices.deviceDisconnected -= OnDeviceDisconnected;

        if (fireAction != null && fireAction.action != null)
        {
            fireAction.action.performed -= OnFire;
            fireAction.action.Disable();
        }
    }

    private void Start()
    {
        if (SpawnLocation == null)
        {
            var spawnObject = GameObject.FindGameObjectWithTag("DartSpawn");
            if (spawnObject != null)
            {
                SpawnLocation = spawnObject.transform;
            }
        }

        if (audioSource == null)
        {
            Debug.LogWarning($"{name}: AudioSource not found on this GameObject. Gun fire sound will not play.");
        }
    }

    private void OnFire(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (IsHeldByInteractor())
        {
            ShootDart();
        }
    }

    private void OnDeviceConnected(InputDevice device)
    {
        InitializeDevices();
    }

    private void OnDeviceDisconnected(InputDevice device)
    {
        InitializeDevices();
    }

    private void InitializeDevices()
    {
        var leftDevices = new List<InputDevice>();
        var rightDevices = new List<InputDevice>();

        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller, leftDevices);
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller, rightDevices);

        if (leftDevices.Count > 0)
            leftDevice = leftDevices[0];

        if (rightDevices.Count > 0)
            rightDevice = rightDevices[0];
    }

    private float lastShot;

    [Range(0, 1024), SerializeField] private float DartSpeed;

    private float DartDelay = 0.2f;

    #region Dart Physics Logic

    public void ShootDart()
    {
        // To know which gun shot
        Debug.Log($"{name} fired");

        if (DartPrefab == null)
        {
            return;
        }

        if (lastShot > Time.time)
        {
            return;
        }

        GameObject NerfDartPrefab = SpawnDart();
        if (NerfDartPrefab == null)
        {
            return;
        }

        lastShot = Time.time + DartDelay;

        ShootDartSound();

        var bulletPrefab = NerfDartPrefab;
        if (bulletPrefab == null)
        {
            return;
        }

        var bulletRB = bulletPrefab.GetComponent<Rigidbody>();
        if (bulletRB == null)
        {
            Debug.LogError($"{name}: Spawned dart prefab does not contain a Rigidbody.");
            return;
        }

        var direction = bulletPrefab.transform.TransformDirection(Vector3.forward);
        bulletRB.AddForce(direction * DartSpeed);
        Destroy(bulletPrefab, 5f);
    }

    private void ShootDartSound()
    {
        if (audioSource == null)
        {
            return;
        }

        var random = Random.Range(0.8f, 1.2f);
        audioSource.pitch = random;
        audioSource.Play();
    }
    #endregion

}

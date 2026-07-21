using UnityEngine;
using UnityEngine.InputSystem;

public class Laser : MonoBehaviour
{
    public GameObject DartPrefab;

    [SerializeField] private InputActionReference fireAction;

    //private Transform SpawnLocation;

    [SerializeField] private Transform SpawnLocation;


    // Code to test gun from laptop
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShootDart();
        }
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
   }

    private void OnEnable()
    {
        if (fireAction != null && fireAction.action != null)
        {
            fireAction.action.performed += OnFire;
            fireAction.action.Enable();
        }
    }

    private void OnDisable()
    {
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

    private void OnFire(InputAction.CallbackContext context)
    {
        ShootDart();
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

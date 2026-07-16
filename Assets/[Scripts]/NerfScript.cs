using UnityEngine;

public class NerfScript : MonoBehaviour
{
    public GameObject LaserPrefab;

    private Transform SpawnLocation;

    private GameObject SpawnDart()
    {
        #region ...
        //check if prefab is null
        if (LaserPrefab == null)
        {
            Debug.Log("empty...");
            return null;
        }
        #endregion

        GameObject Dart = null;

        Debug.Log("LAUNCH THE PROJECTILE!");

        Dart = Instantiate(LaserPrefab, SpawnLocation.position, SpawnLocation.rotation);

        return Dart;
    }


    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        SpawnLocation = GameObject.FindGameObjectWithTag("DartSpawn").transform;

    }

    private float lastShot;

    [Range(0, 1024), SerializeField] private float DartSpeed;

    private float DartDelay = 0.2f;

    #region Dart Physics Logic

    public void ShootDart()
    {
        if (LaserPrefab == null)
        {
            return;
        }

        if (lastShot > Time.time)
        {
            return;
        }

        GameObject NerfLaserPrefab = SpawnDart();

        lastShot = Time.time + DartDelay;

        ShootDartSound();

        //var bulletPrefab = Instantiate(LaserPrefab, DartSpawnPosition.position, DartSpawnPosition.rotation);

        var bulletPrefab = NerfLaserPrefab;

        var bulletRB = bulletPrefab.GetComponent<Rigidbody>();
        var direction = bulletPrefab.transform.TransformDirection(Vector3.forward);
        bulletRB.AddForce(direction * DartSpeed);
        Destroy(bulletPrefab, 5f);


    }

    private void ShootDartSound()
    {
        var random = Random.Range(0.8f, 1.2f);

        audioSource.pitch = random;

        audioSource.Play();
    }
    #endregion
}

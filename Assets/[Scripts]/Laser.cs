using UnityEngine;

public class Laser : MonoBehaviour
{
    public GameObject DartPrefab;

    //private Transform SpawnLocation;

     [SerializeField] private Transform SpawnLocation;


/*Code to test gun from laptop
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShootDart();
        }
    }
*/



    private GameObject SpawnDart()
    {

        Debug.Log($"Spawning from {SpawnLocation.position}");

        #region ...
        //check if prefab is null
        if (DartPrefab == null)
        {
            Debug.Log("empty...");
            return null;
        }
        #endregion

        GameObject Dart = null;

        Debug.Log("LAUNCH THE PROJECTILE!");

        Dart = Instantiate(DartPrefab, SpawnLocation.position, SpawnLocation.rotation);

        return Dart;
    }



/* // Made SpawnLocation a SerializedField at the top, so just need to drag and drop in Unity
    private void Start()
    {
        SpawnLocation = GameObject.FindGameObjectWithTag("DartSpawn").transform;

    }
*/

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

        lastShot = Time.time + DartDelay;

        ShootDartSound();

        //var bulletPrefab = Instantiate(DartPrefab, DartSpawnPosition.position, DartSpawnPosition.rotation);

        var bulletPrefab = NerfDartPrefab;

        var bulletRB = bulletPrefab.GetComponent<Rigidbody>();
        var direction = bulletPrefab.transform.TransformDirection(Vector3.forward);
        bulletRB.AddForce(direction * DartSpeed);
        Destroy(bulletPrefab, 5f);
    }
 #endregion

}

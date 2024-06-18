using UnityEngine;

public class PlayerWeaponHandler : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerNumber;

    [Header("Weapons")]
    public GameObject grenadeLauncher; // Reference to the grenade launcher prefab
    private IShootable currentWeapon;

    private KeyCode shootKey; //what button will trigger the shooting. This is set depending on the player number

    // Start is called before the first frame update
    void Start()
    {
        SetUpInputs();
        ActivateGrenadeLauncher();
    }

    private void Update()
    {
        if (Input.GetKeyDown(shootKey)) Fire();
    }

    private void Fire()
    {
        currentWeapon.Shoot();
    }

    private void ActivateGrenadeLauncher()
    {
        grenadeLauncher.SetActive(true);
        currentWeapon = grenadeLauncher.GetComponent<IShootable>();
    }

    private void SetUpInputs()
    {
        switch (playerNumber)
        {
            case 1:
                shootKey = KeyCode.V;
                break;
            case 2:
                shootKey = KeyCode.Comma;
                break;
            default:
                Debug.Log("No player number assigned. Controls could not be selected");
                break;
        }
    }
}

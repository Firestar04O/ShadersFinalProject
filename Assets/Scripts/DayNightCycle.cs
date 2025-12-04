using UnityEngine;
using UnityEngine.Rendering;

public class DayNightCycle : MonoBehaviour
{
    public Light sun;
    public Volume globalVolume;

    public VolumeProfile dayProfile;
    public VolumeProfile nightProfile;

    public Material windowMaterial;   // El shader animado

    public float dayIntensity = 1f;
    public float nightIntensity = 0.1f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1)) SetNight();
        if (Input.GetKeyDown(KeyCode.Keypad2)) SetDay();
    }

    public void SetDay()
    {
        sun.intensity = dayIntensity;
        globalVolume.profile = dayProfile;

        // Bajar emisión
        windowMaterial.SetFloat("_EmissionStrength", 0.2f);
    }

    public void SetNight()
    {
        sun.intensity = nightIntensity;
        globalVolume.profile = nightProfile;

        // Subir emisión
        windowMaterial.SetFloat("_EmissionStrength", 3f);
    }
}

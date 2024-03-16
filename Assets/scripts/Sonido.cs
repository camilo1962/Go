using UnityEngine;


public class Sonido : MonoBehaviour
{
    public static Sonido instance;


    [SerializeField] private AudioSource voicePlayer;// Definir reproductor de audio
    [SerializeField] private AudioClip ChessDown;
    [SerializeField] private AudioClip ChessUp;
    readonly float voiceVolumeValue = 0.5f;// Volumen predeterminado
    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    public void PlayVoiceChessDown()
    {
        voicePlayer.PlayOneShot(ChessDown);
    }

    public void PlayVoiceChessUp()
    {
        voicePlayer.PlayOneShot(ChessUp);
    }
}
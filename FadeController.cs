using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hace un fade a negro sobre toda la pantalla. Pensado para conectarse
/// al evento "On Ending Reached" del DialogueManager (Inspector, sin código).
/// Colocar en un Image que cubra toda la pantalla, dentro del Canvas,
/// como el último hijo (para que se dibuje encima de todo lo demás).
/// </summary>
[RequireComponent(typeof(Image))]
public class FadeController : MonoBehaviour
{
    [Header("Configuración")]
    public float fadeDuration = 2f;

    [Header("Opcional: qué pasa cuando termina el fade")]
    public bool loadSceneAfterFade = false;
    public string sceneToLoad; // Nombre exacto de la escena (debe estar en Build Settings)

    private Image fadeImage;

    void Awake()
    {
        fadeImage = GetComponent<Image>();
        SetAlpha(0f);
        fadeImage.raycastTarget = false; // No bloquea clics mientras está invisible
    }

    /// <summary>
    /// Llamar desde el evento "On Ending Reached" del DialogueManager.
    /// El parámetro (endingName) no se usa aquí, pero debe existir para que
    /// coincida con la firma del UnityEvent&lt;string&gt;.
    /// </summary>
    public void OnEndingReached(string endingName)
    {
        StartCoroutine(FadeToBlackRoutine());
    }

    private IEnumerator FadeToBlackRoutine()
    {
        fadeImage.raycastTarget = true; // Ahora sí bloquea clics, ya no debe haber interacción
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(1f);

        if (loadSceneAfterFade && !string.IsNullOrEmpty(sceneToLoad))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
        }
    }

    private void SetAlpha(float alpha)
    {
        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
    }
}
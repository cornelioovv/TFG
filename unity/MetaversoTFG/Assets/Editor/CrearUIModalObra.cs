#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// Herramienta de editor: crea de un solo click el Canvas + modal de detalle de obra
/// con toda su jerarquía y referencias pre-wired al componente ObraDetailUI.
///
/// Uso: Menú "TFG → Crear UI Modal Obra".
public static class CrearUIModalObra
{
    static readonly Color BackdropColor = new Color(0f, 0f, 0f, 0.7f);
    static readonly Color BoxColor = new Color(0.98f, 0.98f, 0.98f, 1f);
    static readonly Color TextDark = new Color(0.06f, 0.06f, 0.07f);
    static readonly Color TextMuted = new Color(0.45f, 0.45f, 0.5f);
    static readonly Color AccentPrimary = new Color(0.18f, 0.63f, 0.23f);

    [MenuItem("TFG/Crear UI Modal Obra")]
    public static void Crear()
    {
        // Validaciones
        if (TMP_Settings.instance == null)
        {
            EditorUtility.DisplayDialog(
                "TextMeshPro no importado",
                "Importa primero las TMP Essential Resources:\nWindow → TextMeshPro → Import TMP Essential Resources",
                "OK");
            return;
        }

        // EventSystem (si no hay uno ya)
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem));
            // Si está disponible el InputSystemUIInputModule (New Input System), lo usamos.
            var inputSystemModuleType = System.Type.GetType(
                "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputSystemModuleType != null)
            {
                es.AddComponent(inputSystemModuleType);
            }
            else
            {
                es.AddComponent<StandaloneInputModule>();
            }
            Undo.RegisterCreatedObjectUndo(es, "Crear EventSystem");
        }

        // Canvas
        var canvasGO = new GameObject(
            "UICanvas",
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasGO, "Crear UICanvas");

        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // Backdrop (fullscreen semi-transparente, este es el "panel" que se activa/desactiva)
        var backdrop = CrearImagen("ObraDetailPanel", canvasGO.transform, BackdropColor);
        Stretch(backdrop.GetComponent<RectTransform>());

        // Caja del modal (centrada)
        var box = CrearImagen("ModalBox", backdrop.transform, BoxColor);
        var boxRT = box.GetComponent<RectTransform>();
        boxRT.anchorMin = boxRT.anchorMax = new Vector2(0.5f, 0.5f);
        boxRT.pivot = new Vector2(0.5f, 0.5f);
        boxRT.sizeDelta = new Vector2(720f, 720f);
        boxRT.anchoredPosition = Vector2.zero;
        AddOutline(box, new Color(0.85f, 0.85f, 0.88f), 1f);

        // --- Cabecera ---
        var titulo = CrearTMP("TituloLabel", box.transform, "Título de la obra", 32, TextDark, FontStyles.Bold);
        SetRectTop(titulo.GetComponent<RectTransform>(), 30, 50, 30, 60);
        titulo.alignment = TextAlignmentOptions.Left;

        var autor = CrearTMP("AutorLabel", box.transform, "Autor", 18, TextMuted, FontStyles.Normal);
        SetRectTop(autor.GetComponent<RectTransform>(), 30, 90, 30, 30);
        autor.alignment = TextAlignmentOptions.Left;

        var cerrarBtn = CrearBoton("CerrarButton", box.transform, "✕", 24);
        var cerrarRT = cerrarBtn.GetComponent<RectTransform>();
        cerrarRT.anchorMin = cerrarRT.anchorMax = new Vector2(1f, 1f);
        cerrarRT.pivot = new Vector2(1f, 1f);
        cerrarRT.sizeDelta = new Vector2(48f, 48f);
        cerrarRT.anchoredPosition = new Vector2(-12f, -12f);

        // --- Imagen de la obra ---
        var imgGO = new GameObject("ObraImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        imgGO.transform.SetParent(box.transform, false);
        var rawImage = imgGO.GetComponent<RawImage>();
        rawImage.color = Color.white;
        var imgRT = imgGO.GetComponent<RectTransform>();
        SetRectTop(imgRT, 30, 140, 30, 320);

        // --- Info ---
        var precio = CrearTMP("PrecioLabel", box.transform, "0 €", 28, TextDark, FontStyles.Bold);
        SetRectTop(precio.GetComponent<RectTransform>(), 30, 480, 360, 40);
        precio.alignment = TextAlignmentOptions.Left;

        var estado = CrearTMP("EstadoLabel", box.transform, "DISPONIBLE", 16, AccentPrimary, FontStyles.Bold);
        SetRectTop(estado.GetComponent<RectTransform>(), 360, 480, 30, 40);
        estado.alignment = TextAlignmentOptions.Right;

        // --- Formulario de compra ---
        var formGO = new GameObject("FormularioCompra", typeof(RectTransform));
        formGO.transform.SetParent(box.transform, false);
        var formRT = formGO.GetComponent<RectTransform>();
        SetRectTop(formRT, 30, 530, 30, 130);

        var nombreInput = CrearInputField("NombreInput", formGO.transform, "Nombre del comprador");
        var nombreRT = nombreInput.GetComponent<RectTransform>();
        nombreRT.anchorMin = new Vector2(0f, 1f);
        nombreRT.anchorMax = new Vector2(0.48f, 1f);
        nombreRT.pivot = new Vector2(0f, 1f);
        nombreRT.anchoredPosition = new Vector2(0f, 0f);
        nombreRT.sizeDelta = new Vector2(0f, 38f);

        var emailInput = CrearInputField("EmailInput", formGO.transform, "Email");
        var emailRT = emailInput.GetComponent<RectTransform>();
        emailRT.anchorMin = new Vector2(0.52f, 1f);
        emailRT.anchorMax = new Vector2(1f, 1f);
        emailRT.pivot = new Vector2(0f, 1f);
        emailRT.anchoredPosition = new Vector2(0f, 0f);
        emailRT.sizeDelta = new Vector2(0f, 38f);

        var comprarBtn = CrearBoton("ComprarButton", formGO.transform, "Comprar", 18);
        var comprarRT = comprarBtn.GetComponent<RectTransform>();
        comprarRT.anchorMin = new Vector2(0f, 1f);
        comprarRT.anchorMax = new Vector2(1f, 1f);
        comprarRT.pivot = new Vector2(0.5f, 1f);
        comprarRT.anchoredPosition = new Vector2(0f, -56f);
        comprarRT.sizeDelta = new Vector2(0f, 48f);
        SetButtonColor(comprarBtn, AccentPrimary, Color.white);

        // --- Mensaje de feedback ---
        var mensaje = CrearTMP("MensajeLabel", box.transform, "", 14, TextMuted, FontStyles.Italic);
        SetRectTop(mensaje.GetComponent<RectTransform>(), 30, 680, 30, 24);
        mensaje.alignment = TextAlignmentOptions.Center;

        // Apagar el panel por defecto
        backdrop.SetActive(false);

        // --- Añadir y configurar ObraDetailUI ---
        var ui = canvasGO.AddComponent<ObraDetailUI>();
        ui.panel = backdrop;
        ui.imagen = rawImage;
        ui.tituloLabel = titulo;
        ui.autorLabel = autor;
        ui.precioLabel = precio;
        ui.estadoLabel = estado;
        ui.formularioCompra = formGO;
        ui.nombreInput = nombreInput;
        ui.emailInput = emailInput;
        ui.comprarButton = comprarBtn;
        ui.cerrarButton = cerrarBtn;
        ui.mensajeLabel = mensaje;

        Selection.activeGameObject = canvasGO;
        EditorUtility.DisplayDialog(
            "UI creada",
            "Se ha creado UICanvas con todas las referencias asignadas.\n\n" +
            "Falta un último paso:\n" +
            "1. Selecciona la cámara con ObraInteractor.\n" +
            "2. Arrastra UICanvas al slot 'Detail UI'.",
            "OK");

        Debug.Log("[CrearUIModalObra] UI creada correctamente.");
    }

    // ---------- Helpers ----------

    static GameObject CrearImagen(string name, Transform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = color;
        return go;
    }

    static TextMeshProUGUI CrearTMP(string name, Transform parent, string text, float fontSize, Color color, FontStyles style)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.enableWordWrapping = true;
        return tmp;
    }

    static Button CrearBoton(string name, Transform parent, string label, float fontSize)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = new Color(0.9f, 0.9f, 0.92f);

        var textGo = new GameObject("Text (TMP)", typeof(RectTransform));
        textGo.transform.SetParent(go.transform, false);
        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = fontSize;
        tmp.color = TextDark;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        Stretch(textGo.GetComponent<RectTransform>());

        return go.GetComponent<Button>();
    }

    static TMP_InputField CrearInputField(string name, Transform parent, string placeholder)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = Color.white;
        AddOutline(go, new Color(0.7f, 0.7f, 0.75f), 1f);

        var textArea = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
        textArea.transform.SetParent(go.transform, false);
        Stretch(textArea.GetComponent<RectTransform>(), 8f, 4f, 8f, 4f);

        var placeholderGo = new GameObject("Placeholder", typeof(RectTransform));
        placeholderGo.transform.SetParent(textArea.transform, false);
        var phTmp = placeholderGo.AddComponent<TextMeshProUGUI>();
        phTmp.text = placeholder;
        phTmp.fontSize = 16;
        phTmp.color = new Color(0.6f, 0.6f, 0.65f);
        phTmp.fontStyle = FontStyles.Italic;
        phTmp.alignment = TextAlignmentOptions.Left;
        Stretch(placeholderGo.GetComponent<RectTransform>());

        var textGo = new GameObject("Text", typeof(RectTransform));
        textGo.transform.SetParent(textArea.transform, false);
        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = "";
        tmp.fontSize = 16;
        tmp.color = TextDark;
        tmp.alignment = TextAlignmentOptions.Left;
        Stretch(textGo.GetComponent<RectTransform>());

        var input = go.AddComponent<TMP_InputField>();
        input.textViewport = textArea.GetComponent<RectTransform>();
        input.textComponent = tmp;
        input.placeholder = phTmp;
        input.targetGraphic = img;

        return input;
    }

    static void SetButtonColor(Button btn, Color bg, Color text)
    {
        var img = btn.GetComponent<Image>();
        if (img != null) img.color = bg;
        var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null) tmp.color = text;
    }

    static void AddOutline(GameObject go, Color color, float thickness)
    {
        var outline = go.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = color;
        outline.effectDistance = new Vector2(thickness, -thickness);
    }

    /// Posiciona un RectTransform anclado al borde SUPERIOR de su padre.
    /// `left/right` = padding lateral; `top` = distancia desde el borde superior; `height` = alto.
    static void SetRectTop(RectTransform rt, float left, float top, float right, float height)
    {
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.offsetMin = new Vector2(left, -(top + height));
        rt.offsetMax = new Vector2(-right, -top);
    }

    /// Estira un RectTransform para cubrir todo su padre con opcional padding.
    static void Stretch(RectTransform rt, float left = 0f, float top = 0f, float right = 0f, float bottom = 0f)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, -top);
    }
}
#endif

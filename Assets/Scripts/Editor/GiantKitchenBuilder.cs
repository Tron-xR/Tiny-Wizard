using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class GiantKitchenBuilder
{
    private const float PHEIGHT = 1.8f; // player height reference
    private static Transform root;

    [MenuItem("Tiny Wizard/Build Giant Kitchen")]
    public static void BuildKitchen()
    {
        GameObject kitchenRoot = new GameObject("KitchenRoot");
        root = kitchenRoot.transform;

        BuildRoom();
        BuildCounters();
        BuildSink();
        BuildStove();
        BuildFridge();
        BuildUnderCounter();
        BuildCounterObjects();
        BuildFloorObjects();
        BuildLighting();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Kitchen Built",
            "Giant kitchen created!\n~50 primitive objects under KitchenRoot.\n\nScale reference:\n- Fork = player height (1.8m)\n- Mug = player can stand inside\n- Fridge = building-sized", "OK");
    }

    // ─── ROOM ───

    private static void BuildRoom()
    {
        Floor("Floor", 24, 0.2f, 16, new Vector3(0, 0, 8), 0.83f, 0.78f, 0.69f);
        Box("BackWall", 24, 12, 0.3f, new Vector3(0, 6, 0), 0.91f, 0.88f, 0.83f);
        Box("LeftWall", 0.3f, 12, 16, new Vector3(-12, 6, 8), 0.91f, 0.88f, 0.83f);
        Box("RightWall", 0.3f, 12, 16, new Vector3(12, 6, 8), 0.91f, 0.88f, 0.83f);

        Box("Wall_LeftSegment", 8, 6, 0.3f, new Vector3(-8, 9, 0), 0.91f, 0.88f, 0.83f);
        Box("Wall_RightSegment", 8, 6, 0.3f, new Vector3(8, 9, 0), 0.91f, 0.88f, 0.83f);
        Box("Wall_TopSegment", 24, 2, 0.3f, new Vector3(0, 13, 0), 0.91f, 0.88f, 0.83f);

        // Window glass
        Box("WindowGlass", 8, 6, 0.05f, new Vector3(0, 9, 0.2f), 0.6f, 0.8f, 1f);
    }

    // ─── COUNTERS ───

    private static void BuildCounters()
    {
        Transform g = Group("CounterTopArea");

        Box("Counter_Left", 8, 2, 6, new Vector3(-4, 1, 2), 0.55f, 0.41f, 0.08f, g);
        Box("Counter_Center", 4, 2, 6, new Vector3(0, 1, 2), 0.55f, 0.41f, 0.08f, g);
        Box("Counter_Right", 8, 2, 6, new Vector3(4, 1, 2), 0.55f, 0.41f, 0.08f, g);

        Box("Top_Left", 8.4f, 0.15f, 6.4f, new Vector3(-4, 2.075f, 2), 0.65f, 0.49f, 0.12f, g);
        Box("Top_Center", 4.4f, 0.15f, 6.4f, new Vector3(0, 2.075f, 2), 0.65f, 0.49f, 0.12f, g);
        Box("Top_Right", 8.4f, 0.15f, 6.4f, new Vector3(4, 2.075f, 2), 0.65f, 0.49f, 0.12f, g);
    }

    // ─── SINK ───

    private static void BuildSink()
    {
        Transform g = Group("SinkArea");
        float cy = 2f, cz = 2f;

        Cyl("Sink_Bowl", 3.5f, 1f, new Vector3(0, cy + 0.1f, cz), 0.75f, 0.75f, 0.78f, g);
        Cyl("Faucet_Base", 0.3f, 0.3f, new Vector3(1.5f, cy + 0.15f, cz - 1f), 0.8f, 0.8f, 0.85f, g);
        Cyl("Faucet_Pipe", 0.12f, 2f, new Vector3(1.5f, cy + 0.8f, cz - 1f), 0.8f, 0.8f, 0.85f, g);
        Sph("Faucet_Spout", 0.15f, new Vector3(1.5f, cy + 1.8f, cz - 1.5f), 0.8f, 0.8f, 0.85f, g);

        Sph("Knob_Left", 0.2f, new Vector3(2.2f, cy + 0.4f, cz + 1.5f), 1f, 0.2f, 0.2f, g);
        Sph("Knob_Right", 0.2f, new Vector3(2.8f, cy + 0.4f, cz + 1.5f), 0.2f, 0.4f, 1f, g);

        Box("Sponge", 1f, 0.4f, 0.6f, new Vector3(-1.5f, cy + 0.25f, cz + 1.5f), 1f, 0.88f, 0.25f, g);
        Sph("WaterDrop", 0.08f, new Vector3(1.5f, cy + 0.5f, cz - 1.8f), 0.5f, 0.7f, 1f, g);
    }

    // ─── STOVE ───

    private static void BuildStove()
    {
        Transform g = Group("StoveArea");
        float cx = 4, cy = 2f, cz = 2f;

        Box("Body", 7f, 0.2f, 5f, new Vector3(cx, cy + 0.1f, cz), 0.16f, 0.16f, 0.16f, g);

        float br = 0.6f, ox = 1.5f, oz = 1.2f;
        float bz = 0.05f;
        Cyl("Burner_FL", br, bz, new Vector3(cx - ox, cy + 0.22f, cz - oz), 0.25f, 0.25f, 0.27f, g);
        Cyl("Burner_FR", br, bz, new Vector3(cx + ox, cy + 0.22f, cz - oz), 0.25f, 0.25f, 0.27f, g);
        Cyl("Burner_BL", br, bz, new Vector3(cx - ox, cy + 0.22f, cz + oz), 0.25f, 0.25f, 0.27f, g);
        Cyl("Burner_BR", br, bz, new Vector3(cx + ox, cy + 0.22f, cz + oz), 0.25f, 0.25f, 0.27f, g);

        for (int i = 0; i < 4; i++)
            Sph("Knob_" + (i + 1), 0.15f, new Vector3(cx - 2.5f + i * 1.5f, cy + 0.3f, cz + 2.8f), 0.5f, 0.5f, 0.5f, g);
    }

    // ─── FRIDGE ───

    private static void BuildFridge()
    {
        Transform g = Group("FridgeArea");
        float fx = 11f, fz = 6f;

        Box("Body", 6f, 18f, 4f, new Vector3(fx, 9, fz), 0.94f, 0.94f, 0.94f, g);
        Box("Door", 5.6f, 16f, 0.15f, new Vector3(fx + 3.1f, 9.5f, fz), 0.96f, 0.96f, 0.96f, g);
        Cyl("Handle", 0.08f, 1.5f, new Vector3(fx + 3.2f, 9.5f, fz + 0.4f), 0.6f, 0.6f, 0.6f, g);
        Box("Vent", 4f, 0.5f, 0.1f, new Vector3(fx, 0.5f, fz + 2.1f), 0.3f, 0.3f, 0.3f, g);
        Sph("Magnet", 0.25f, new Vector3(fx + 2f, 12f, fz + 2.2f), 1f, 0.4f, 0.2f, g);
    }

    // ─── UNDER COUNTER ───

    private static void BuildUnderCounter()
    {
        Transform g = Group("UnderCounterArea");
        float cy = 1f;

        Box("Door_Left", 3.6f, 1.6f, 0.1f, new Vector3(-4, cy, 4.3f), 0.45f, 0.35f, 0.1f, g);
        Sph("Knob_Left", 0.1f, new Vector3(-4 + 1.5f, cy, 4.5f), 0.7f, 0.6f, 0.3f, g);
        Box("Door_Right", 3.6f, 1.6f, 0.1f, new Vector3(4, cy, 4.3f), 0.45f, 0.35f, 0.1f, g);
        Sph("Knob_Right", 0.1f, new Vector3(4 - 1.5f, cy, 4.5f), 0.7f, 0.6f, 0.3f, g);
        Box("Drawer_Front", 3.6f, 0.6f, 0.1f, new Vector3(0, 0.5f, 4.3f), 0.5f, 0.4f, 0.12f, g);
        Cyl("Drawer_Handle", 0.05f, 0.6f, new Vector3(0, 0.5f, 4.5f), 0.7f, 0.6f, 0.3f, g);
    }

    // ─── COUNTERTOP OBJECTS ───

    private static void BuildCounterObjects()
    {
        Transform g = Group("CountertopObjects");
        float y = 2.15f;

        // Cutting board
        Box("CuttingBoard", 4f, 0.1f, 2.5f, new Vector3(-5, y, 2.5f), 0.55f, 0.35f, 0.1f, g);

        // Fork (2m long = player height)
        Cyl("Fork_Handle", 0.08f, 1.2f, new Vector3(-3.5f, y + 0.6f, 0.5f), 0.7f, 0.7f, 0.72f, g);
        for (int i = 0; i < 4; i++)
            Box("Fork_Prong_" + (i + 1), 0.05f, 0.8f, 0.03f, new Vector3(-3.5f + (i - 1.5f) * 0.1f, y + 1.6f, 0.5f), 0.7f, 0.7f, 0.72f, g);

        // Spoon (1.8m)
        Cyl("Spoon_Handle", 0.06f, 1.2f, new Vector3(-3.5f, y + 0.6f, 1.8f), 0.7f, 0.7f, 0.72f, g);
        Sph("Spoon_Bowl", 0.25f, new Vector3(-3.5f, y + 1.35f, 2f), 0.7f, 0.7f, 0.72f, g);

        // Knife (2.2m)
        Cyl("Knife_Handle", 0.06f, 0.8f, new Vector3(-3.5f, y + 0.4f, 3f), 0.4f, 0.3f, 0.15f, g);
        Box("Knife_Blade", 1.4f, 0.3f, 0.03f, new Vector3(-3.5f, y + 1.1f, 3f), 0.6f, 0.6f, 0.62f, g);

        // Mug (player fits inside)
        Cyl("Mug_Body", 0.6f, 1.5f, new Vector3(0, y + 0.75f, 1f), 0.8f, 0.2f, 0.2f, g);
        Cyl("Mug_Interior", 0.55f, 1.4f, new Vector3(0, y + 0.75f, 1f), 0.5f, 0.12f, 0.12f, g);
        // Handle (stretched sphere ring approximation)
        Sph("Mug_Handle_A", 0.06f, new Vector3(0.6f, y + 1.1f, 1f), 0.8f, 0.2f, 0.2f, g);
        Sph("Mug_Handle_B", 0.06f, new Vector3(0.6f, y + 0.8f, 1f), 0.8f, 0.2f, 0.2f, g);
        Sph("Mug_Handle_C", 0.06f, new Vector3(0.6f, y + 0.95f, 0.8f), 0.8f, 0.2f, 0.2f, g);
        Sph("Mug_Handle_D", 0.06f, new Vector3(0.6f, y + 0.95f, 1.2f), 0.8f, 0.2f, 0.2f, g);

        // Toaster
        Box("Toaster_Body", 2f, 1.2f, 1.8f, new Vector3(4, y + 0.6f, 1.5f), 0.27f, 0.53f, 0.67f, g);
        Box("Toaster_Slot_1", 1.2f, 0.05f, 0.12f, new Vector3(3.6f, y + 1.2f, 1.5f), 0.1f, 0.1f, 0.1f, g);
        Box("Toaster_Slot_2", 1.2f, 0.05f, 0.12f, new Vector3(4.4f, y + 1.2f, 1.5f), 0.1f, 0.1f, 0.1f, g);
        Sph("Toaster_Knob", 0.08f, new Vector3(4.8f, y + 0.3f, 2.4f), 0.5f, 0.5f, 0.5f, g);

        // Bread
        Box("Bread_Slice", 1.2f, 0.15f, 1.2f, new Vector3(4.5f, y + 0.1f, 2.5f), 0.83f, 0.63f, 0.31f, g);
    }

    // ─── FLOOR DECOR ───

    private static void BuildFloorObjects()
    {
        Transform g = Group("FloorObjects");
        Sph("Crumb_1", 0.08f, new Vector3(-2, 0.08f, 6), 0.5f, 0.35f, 0.2f, g);
        Sph("Crumb_2", 0.06f, new Vector3(-1.5f, 0.06f, 6.5f), 0.5f, 0.35f, 0.2f, g);
        Sph("Crumb_3", 0.1f, new Vector3(3, 0.1f, 5), 0.5f, 0.35f, 0.2f, g);
        Sph("Crumb_4", 0.05f, new Vector3(3.5f, 0.05f, 4.5f), 0.5f, 0.35f, 0.2f, g);
        Sph("Crumb_5", 0.07f, new Vector3(5, 0.07f, 7), 0.5f, 0.35f, 0.2f, g);
        Sph("WaterDroplet", 0.12f, new Vector3(0.5f, 0.12f, 4.5f), 0.5f, 0.7f, 1f, g);
    }

    // ─── LIGHTING ───

    private static void BuildLighting()
    {
        Transform g = Group("_Lighting");

        Light sun = AddLight("DirectionalLight (Sun)", g, LightType.Directional);
        sun.intensity = 1.2f;
        sun.color = new Color(1f, 0.85f, 0.62f);
        sun.shadows = LightShadows.Soft;
        sun.gameObject.transform.eulerAngles = new Vector3(50, -30, 0);

        Light fill = AddLight("FillLight (Point)", g, LightType.Point);
        fill.intensity = 0.4f;
        fill.color = new Color(0.7f, 0.8f, 1f);
        fill.range = 20f;
        fill.shadows = LightShadows.None;
        fill.gameObject.transform.localPosition = new Vector3(0, 14, 5);

        Light stove = AddLight("StoveLight (Accent)", g, LightType.Point);
        stove.intensity = 0.3f;
        stove.color = new Color(1f, 0.6f, 0.2f);
        stove.range = 6f;
        stove.gameObject.transform.localPosition = new Vector3(4, 3, 2);

        Light fridge = AddLight("FridgeGlow", g, LightType.Point);
        fridge.intensity = 0.5f;
        fridge.color = new Color(0.9f, 0.95f, 1f);
        fridge.range = 4f;
        fridge.gameObject.transform.localPosition = new Vector3(11, 10, 6);
    }

    // ─── PRIMITIVE HELPERS ───

    private static Transform Group(string name)
    {
        GameObject g = new GameObject(name);
        g.transform.SetParent(root, false);
        return g.transform;
    }

    private static GameObject Prim(PrimitiveType type, string name, Transform parent)
    {
        GameObject g = GameObject.CreatePrimitive(type);
        g.name = name;
        g.transform.SetParent(parent, false);
        g.isStatic = true;
        return g;
    }

    private static void Floor(string name, float w, float h, float d, Vector3 pos, float r, float g, float b)
    {
        GameObject go = Prim(PrimitiveType.Cube, name, root);
        go.transform.localPosition = pos;
        go.transform.localScale = new Vector3(w, h, d);
        Mat(go, r, g, b);
    }

    private static void Box(string name, float w, float h, float d, Vector3 pos, float r, float g, float b, Transform parent = null)
    {
        GameObject go = Prim(PrimitiveType.Cube, name, parent ?? root);
        go.transform.localPosition = pos;
        go.transform.localScale = new Vector3(w, h, d);
        Mat(go, r, g, b);
    }

    private static void Cyl(string name, float radius, float height, Vector3 pos, float r, float g, float b, Transform parent = null)
    {
        GameObject go = Prim(PrimitiveType.Cylinder, name, parent ?? root);
        go.transform.localPosition = pos;
        go.transform.localScale = new Vector3(radius * 2, height, radius * 2);
        Mat(go, r, g, b);
    }

    private static void Sph(string name, float radius, Vector3 pos, float r, float g, float b, Transform parent = null)
    {
        GameObject go = Prim(PrimitiveType.Sphere, name, parent ?? root);
        go.transform.localPosition = pos;
        go.transform.localScale = Vector3.one * radius * 2;
        Mat(go, r, g, b);
    }

    private static Light AddLight(string name, Transform parent, LightType type)
    {
        GameObject g = new GameObject(name);
        g.transform.SetParent(parent, false);
        Light l = g.AddComponent<Light>();
        l.type = type;
        return l;
    }

    private static void Mat(GameObject go, float r, float g, float b)
    {
        Renderer rend = go.GetComponent<Renderer>();
        if (rend == null) return;
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat == null)
            mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(r, g, b);
        rend.sharedMaterial = mat;
    }
}

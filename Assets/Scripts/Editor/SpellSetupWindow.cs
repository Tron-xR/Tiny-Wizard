using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SpellSetupWindow : EditorWindow
{
    [MenuItem("Tools/Spell System Setup")]
    public static void ShowWindow()
    {
        GetWindow<SpellSetupWindow>("Spell System Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("Step 1: Spell Manager", EditorStyles.boldLabel);
        if (GUILayout.Button("Add SpellManager to Player"))
        {
            SetupSpellManager();
        }

        GUILayout.Space(10);
        GUILayout.Label("Step 2: Spell Child Objects", EditorStyles.boldLabel);
        if (GUILayout.Button("Create Spell Child Objects"))
        {
            CreateSpellObjects();
        }

        GUILayout.Space(10);
        GUILayout.Label("Step 3: CastOrigin on Model", EditorStyles.boldLabel);
        if (GUILayout.Button("Create CastOrigin on Model"))
        {
            CreateCastOrigin();
        }

        GUILayout.Space(10);
        GUILayout.Label("Step 4: Create Prefabs", EditorStyles.boldLabel);
        if (GUILayout.Button("Create All Prefabs"))
        {
            CreateAllPrefabs();
        }

        GUILayout.Space(10);
        GUILayout.Label("Step 5: Assign Prefabs to Spells", EditorStyles.boldLabel);
        if (GUILayout.Button("Assign Prefabs to Spell Objects"))
        {
            AssignPrefabsToSpells();
        }

        GUILayout.Space(10);
        GUILayout.Label("Status", EditorStyles.boldLabel);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            EditorGUILayout.HelpBox("Player found.", MessageType.Info);

            SpellManager sm = player.GetComponent<SpellManager>();
            EditorGUILayout.LabelField("SpellManager:", sm != null ? "OK" : "Missing");

            Transform castOrigin = player.transform.Find("Model Cube/CastOrigin");
            if (castOrigin == null) castOrigin = player.transform.Find("CastOrigin");
            EditorGUILayout.LabelField("CastOrigin:", castOrigin != null ? "OK" : "Missing");

            Animator anim = player.GetComponent<Animator>();
            EditorGUILayout.LabelField("Animator:", anim != null ? "OK" : "Missing");

            if (sm != null)
            {
                SerializedObject so = new SerializedObject(sm);
                SerializedProperty spellList = so.FindProperty("spells");
                if (spellList != null) EditorGUILayout.LabelField("Spell Count:", spellList.arraySize.ToString());
                if (so.FindProperty("castOrigin").objectReferenceValue != null) EditorGUILayout.LabelField("castOrigin ref:", "Set");
                if (so.FindProperty("playerAnimator").objectReferenceValue != null) EditorGUILayout.LabelField("playerAnimator ref:", "Set");
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No player found with 'Player' tag.", MessageType.Warning);
        }

        UnityEngine.GameObject projPrefab = AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/Prefabs/SpellProjectile.prefab");
        UnityEngine.GameObject bouncePrefab = AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/Prefabs/BouncePad.prefab");
        UnityEngine.GameObject icePrefab = AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/Prefabs/IcePlatform.prefab");
        EditorGUILayout.LabelField("SpellProjectile prefab:", projPrefab != null ? "OK" : "Missing");
        EditorGUILayout.LabelField("BouncePad prefab:", bouncePrefab != null ? "OK" : "Missing");
        EditorGUILayout.LabelField("IcePlatform prefab:", icePrefab != null ? "OK" : "Missing");
    }

    private void SetupSpellManager()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("No GameObject with 'Player' tag found.");
            return;
        }

        SpellManager sm = player.GetComponent<SpellManager>();
        if (sm == null)
            sm = player.AddComponent<SpellManager>();

        PlayerInputHandler input = player.GetComponent<PlayerInputHandler>();
        Animator anim = player.GetComponent<Animator>();

        SerializedObject so = new SerializedObject(sm);
        if (input != null)
            so.FindProperty("inputHandler").objectReferenceValue = input;
        if (anim != null)
            so.FindProperty("playerAnimator").objectReferenceValue = anim;
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("SpellManager added to Player with references!");
    }

    private void CreateSpellObjects()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        SpellManager sm = player.GetComponent<SpellManager>();
        if (sm == null) return;

        string[] spellNames = { "AttackSpell", "FreezeSpell", "BounceSpell" };
        System.Type[] spellTypes = { typeof(AttackSpell), typeof(FreezeSpell), typeof(BounceSpell) };

        SerializedObject so = new SerializedObject(sm);
        SerializedProperty spellList = so.FindProperty("spells");
        spellList.ClearArray();

        for (int i = 0; i < spellNames.Length; i++)
        {
            Transform existing = player.transform.Find(spellNames[i]);
            GameObject spellGO;
            if (existing != null)
                spellGO = existing.gameObject;
            else
            {
                spellGO = new GameObject(spellNames[i]);
                spellGO.transform.SetParent(player.transform);
                spellGO.transform.localPosition = Vector3.zero;
                spellGO.transform.localRotation = Quaternion.identity;
            }

            Component comp = spellGO.GetComponent(spellTypes[i]);
            if (comp == null)
                comp = spellGO.AddComponent(spellTypes[i]);

            spellList.InsertArrayElementAtIndex(i);
            spellList.GetArrayElementAtIndex(i).objectReferenceValue = comp;
        }

        so.ApplyModifiedProperties();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Spell child objects created and assigned!");
    }

    private void CreateCastOrigin()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Transform modelCube = player.transform.Find("Model Cube");
        if (modelCube == null)
        {
            Debug.LogError("No 'Model Cube' child found on Player.");
            return;
        }

        Transform existing = modelCube.Find("CastOrigin");
        if (existing == null)
        {
            GameObject castOrigin = new GameObject("CastOrigin");
            castOrigin.transform.SetParent(modelCube);
            castOrigin.transform.localPosition = new Vector3(0, 0.75f, 0.5f);
            castOrigin.transform.localRotation = Quaternion.identity;
        }

        SpellManager sm = player.GetComponent<SpellManager>();
        if (sm != null)
        {
            SerializedObject so = new SerializedObject(sm);
            so.FindProperty("castOrigin").objectReferenceValue = modelCube.Find("CastOrigin");
            so.ApplyModifiedProperties();
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("CastOrigin created on Model Cube!");
    }

    private void CreateAllPrefabs()
    {
        EnsurePrefabDirectory();
        CreateSpellProjectilePrefab();
        CreateBouncePadPrefab();
        CreatePadPrefab("IcePlatform", new Vector3(2f, 0.15f, 2f));
        AssetDatabase.Refresh();
        Debug.Log("All prefabs created!");
    }

    private void EnsurePrefabDirectory()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
    }

    private void CreateSpellProjectilePrefab()
    {
        string path = "Assets/Prefabs/SpellProjectile.prefab";
        UnityEngine.GameObject existing = AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(path);
        if (existing != null)
            AssetDatabase.DeleteAsset(path);

        UnityEngine.GameObject go = UnityEngine.GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "SpellProjectile";
        Object.DestroyImmediate(go.GetComponent<Collider>());

        SphereCollider col = go.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 0.5f;

        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb == null) rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        go.AddComponent<SpellProjectile>();

        go.transform.localScale = Vector3.one * 0.3f;

        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        Debug.Log("Created SpellProjectile.prefab");
    }

    private void CreateBouncePadPrefab()
    {
        string path = "Assets/Prefabs/BouncePad.prefab";
        UnityEngine.GameObject existing = AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(path);
        if (existing != null)
            AssetDatabase.DeleteAsset(path);

        UnityEngine.GameObject go = UnityEngine.GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "BouncePad";
        go.transform.localScale = new Vector3(1.5f, 0.1f, 1.5f);

        BoxCollider col = go.GetComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(3, 20, 3);

        go.AddComponent<BouncePad>();

        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        Debug.Log("Created BouncePad.prefab with BouncePad script and trigger.");
    }

    private void CreatePadPrefab(string name, Vector3 scale)
    {
        string path = $"Assets/Prefabs/{name}.prefab";
        UnityEngine.GameObject existing = AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(path);
        if (existing != null)
            AssetDatabase.DeleteAsset(path);

        UnityEngine.GameObject go = UnityEngine.GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.localScale = scale;

        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        Debug.Log($"Created {name}.prefab");
    }

    private void AssignPrefabsToSpells()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        AttackSpell attack = player.GetComponentInChildren<AttackSpell>();
        FreezeSpell freeze = player.GetComponentInChildren<FreezeSpell>();
        BounceSpell bounce = player.GetComponentInChildren<BounceSpell>();

        UnityEngine.GameObject projPrefab = AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/Prefabs/SpellProjectile.prefab");
        UnityEngine.GameObject bouncePrefab = AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/Prefabs/BouncePad.prefab");
        UnityEngine.GameObject icePrefab = AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/Prefabs/IcePlatform.prefab");

        if (attack != null && projPrefab != null)
        {
            SerializedObject so = new SerializedObject(attack);
            so.FindProperty("projectilePrefab").objectReferenceValue = projPrefab;
            so.ApplyModifiedProperties();
        }

        if (freeze != null && icePrefab != null)
        {
            SerializedObject so = new SerializedObject(freeze);
            so.FindProperty("icePlatformPrefab").objectReferenceValue = icePrefab;
            so.ApplyModifiedProperties();
        }

        if (bounce != null && bouncePrefab != null)
        {
            SerializedObject so = new SerializedObject(bounce);
            so.FindProperty("bouncePadPrefab").objectReferenceValue = bouncePrefab;
            so.ApplyModifiedProperties();
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Prefab references assigned to spell objects!");
    }
}

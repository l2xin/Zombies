using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GlobalEditorWindow : EditorWindow {
	public static GlobalEditorWindow globalEditorWindow;
	private GlobalInfo globalInfo;
	private Vector2 scrollPos;

	private bool advancedOptions;
	private bool preloadOptions;
	private bool cameraOptions;
	private bool characterRotationOptions;
	private bool fontOptions;
	private bool languageOptions;
	private bool announcerOptions;
	private bool guiOptions;
	private bool guiScreensOptions;
	private bool screenOptions;
	private bool roundOptions;
	private bool bounceOptions;
	private bool counterHitOptions;
	private bool comboOptions;
	private bool debugOptions;
	private bool aiOptions;
	private bool blockOptions;
	private bool knockDownOptions;
	private bool sweepOptions;
	private bool hitOptions;
	private bool inputsOptions;
	private bool networkOptions;
	private bool player1InputOptions;
	private bool player2InputOptions;
	private bool cInputOptions;
	private bool touchControllerOptions;
	private bool stageOptions;
    private bool storyModeOptions;
    private bool trainingModeOptions;
	private bool storyModeSelectableCharactersInStoryModeOptions;
	private bool storyModeSelectableCharactersInVersusModeOptions;
	private bool characterOptions;
    private bool updateConfirm;
    private bool lobbyOptions;
    private GameObject canvasPreview;
    private GameObject eventSystemPreview;
    private BaseUI screenPreview;
	
	private string titleStyle;
	private string addButtonStyle;
	private string borderBarStyle;
	private string rootGroupStyle;
	private string fillBarStyle1;
	private string subGroupStyle;
	private string arrayElementStyle;
	private string subArrayElementStyle;
	private string foldStyle;
	private string enumStyle;
	private GUIStyle labelStyle;
	private GUIContent helpGUIContent = new GUIContent();
    private string pName;
    private bool bundlePack;

    private bool skinCharactersOptions;
	private bool levelCharactersOptions;

	private bool meleeWeaponsOptions;
	private bool bowsOptions;
	private bool shieldsOptions;

    [MenuItem("Window/Global Editor")]
	public static void Init(){
		globalEditorWindow = EditorWindow.GetWindow<GlobalEditorWindow>(false, "Global", true);
		globalEditorWindow.Show();
		globalEditorWindow.Populate();
	}
	
	void OnSelectionChange(){
		Populate();
		Repaint();
	}
	
	void OnEnable(){
		Populate();
	}
	
	void OnFocus(){
		Populate();
	}
	
	void OnDisable(){
		Clear();
	}
	
	void OnDestroy(){
		Clear();
	}
	
	void OnLostFocus(){
		//Clear();
	}
	
	void Update(){
		if (EditorApplication.isPlayingOrWillChangePlaymode) {
			Clear();
		}
	}

	void Clear(){
		if (globalInfo != null){
            CloseGUICanvas();
		}
	}

	void helpButton(string page){
		if (GUILayout.Button("?", GUILayout.Width(18), GUILayout.Height(18))) 
			Application.OpenURL("http://www.ufe3d.com/doku.php/"+ page);
	}

    void Populate() {
        this.titleContent = new GUIContent("Global", (Texture)Resources.Load("Icons/Global"));

		// Style Definitions
		titleStyle = "MeTransOffRight";
		borderBarStyle = "ProgressBarBack";
		addButtonStyle = "CN CountBadge";
		rootGroupStyle = "GroupBox";
		subGroupStyle = "ObjectFieldThumb";
		arrayElementStyle = "flow overlay box";
		fillBarStyle1 = "ProgressBarBar";
		subArrayElementStyle = "HelpBox";
		foldStyle = "Foldout";
		enumStyle = "MiniPopup";
		
		labelStyle = new GUIStyle();
		labelStyle.alignment = TextAnchor.MiddleCenter;
		labelStyle.fontStyle = FontStyle.Bold;
		labelStyle.normal.textColor = Color.white;


		helpGUIContent.text = "";
		helpGUIContent.tooltip = "Open Live Docs";
		//helpGUIContent.image = (Texture2D) EditorGUIUtility.Load("icons/SVN_Local.png");
		
		UnityEngine.Object[] selection = Selection.GetFiltered(typeof(GlobalInfo), SelectionMode.Assets);
		if (selection.Length > 0){
			if (selection[0] == null) return;
			globalInfo = (GlobalInfo) selection[0];
		}
		
		/*UFE.isCInputInstalled = UFE.IsInstalled("cInput");
		UFE.isControlFreakInstalled = UFE.IsInstalled("TouchController");
		UFE.isAiAddonInstalled = UFE.IsInstalled("RuleBasedAI");
        UFE.isNetworkAddonInstalled = UFE.IsInstalled("NetworkController");*/
	}

    private void basicMoveUpdate(BasicMoveInfo basicMove, WrapMode wrapMode, bool autoSpeed) {
        basicMove.wrapMode = wrapMode;
        basicMove.autoSpeed = autoSpeed;
    }

	public void OnGUI(){
		if (globalInfo == null){
			GUILayout.BeginHorizontal("GroupBox");
            GUILayout.Label("Select a Global Configuration file\nor create a new one.", "CN EntryInfo");
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();
			if (GUILayout.Button("Create new Global Configuration"))
				ScriptableObjectUtility.CreateAsset<CharacterInfo> ();
			return;
		}


		GUIStyle fontStyle = new GUIStyle();
        //fontStyle.font = (Font)EditorGUIUtility.Load("EditorFont.TTF");
        fontStyle.font = (Font)Resources.Load("EditorFont");
		fontStyle.fontSize = 30;
		fontStyle.alignment = TextAnchor.UpperCenter;
		fontStyle.normal.textColor = Color.white;
		fontStyle.hover.textColor = Color.white;
		EditorGUILayout.BeginVertical(titleStyle);{
			EditorGUILayout.BeginHorizontal();{
				EditorGUILayout.LabelField("", (globalInfo.gameName == "" ? "Universal Fighting Engine" : globalInfo.gameName) , fontStyle, GUILayout.Height(32));
				helpButton("global:start");
			}EditorGUILayout.EndHorizontal();
		}EditorGUILayout.EndVertical();
		
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);{
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUIUtility.labelWidth = 120;
				globalInfo.gameName = EditorGUILayout.TextField("Project Name:", globalInfo.gameName);
				EditorGUILayout.Space();
				EditorGUIUtility.labelWidth = 200;
				EditorGUILayout.Space();

				EditorGUIUtility.labelWidth = 150;
			}EditorGUILayout.EndVertical();


            // Normal Options
            EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					debugOptions = EditorGUILayout.Foldout(debugOptions, "Normal Options", foldStyle);
					helpButton("global:debugoptions");
				}EditorGUILayout.EndHorizontal();
				
				if (debugOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUI.indentLevel += 1;
						EditorGUILayout.Space();
						EditorGUIUtility.labelWidth = 220;
                        
                        EditorGUILayout.Space();
                        
                        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

                        globalInfo.selectedStage = globalInfo.stages[0];

                        GUILayout.Label("Default");

                        globalInfo.p1CharStorage = (CharacterInfo)EditorGUILayout.ObjectField("Player1 Character:", globalInfo.p1CharStorage, typeof(CharacterInfo), false);
                        globalInfo.p2CharStorage = (CharacterInfo)EditorGUILayout.ObjectField("Player2 Character:", globalInfo.p2CharStorage, typeof(CharacterInfo), false);

                        EditorGUILayout.Space();
						
						EditorGUIUtility.labelWidth = 150;
						EditorGUI.indentLevel -= 1;

					}EditorGUILayout.EndVertical();
				}
			}EditorGUILayout.EndVertical();

            // Skin
            EditorGUILayout.BeginVertical(rootGroupStyle);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    skinCharactersOptions = EditorGUILayout.Foldout(skinCharactersOptions, "SkinCharacters (" + globalInfo.skinCharacters.Length + ")", EditorStyles.foldout);
                }
                EditorGUILayout.EndHorizontal();

                if (skinCharactersOptions)
                {
                    EditorGUILayout.BeginVertical(subGroupStyle);
                    {
                        EditorGUI.indentLevel += 1;
                        for (int i = 0; i < globalInfo.skinCharacters.Length; i++)
                        {
                            EditorGUILayout.Space();
                            EditorGUILayout.BeginVertical(arrayElementStyle);
                            {
                                EditorGUILayout.Space();
                                globalInfo.skinCharacters[i] = (CharacterInfo)EditorGUILayout.ObjectField("CharStorageSkin:", globalInfo.skinCharacters[i], typeof(CharacterInfo), false);
                                if (GUILayout.Button("", "PaneOptions"))
                                {
                                    PaneOptions<CharacterInfo>(globalInfo.skinCharacters, globalInfo.skinCharacters[i], delegate (CharacterInfo[] newElement) { globalInfo.skinCharacters = newElement; });
                                }
                                EditorGUILayout.Space();
                            }
                            EditorGUILayout.EndVertical();
                        }
                        EditorGUI.indentLevel -= 1;
                    }
                    EditorGUILayout.EndVertical();
                }
                if (StyledButton("New skinCharacter"))
                    globalInfo.skinCharacters = AddElement<CharacterInfo>(globalInfo.skinCharacters, null);
            }
            EditorGUILayout.EndVertical();

			// level monsters
			EditorGUILayout.BeginVertical(rootGroupStyle);
			{
				EditorGUILayout.BeginHorizontal();
				{
					levelCharactersOptions = EditorGUILayout.Foldout(levelCharactersOptions, "LevelMonstersOptions (" + globalInfo.levelMonsterCharacters.Length + ")", EditorStyles.foldout);
				}
				EditorGUILayout.EndHorizontal();

				if (levelCharactersOptions)
				{
					EditorGUILayout.BeginVertical(subGroupStyle);
					{
						EditorGUI.indentLevel += 1;
						for (int i = 0; i < globalInfo.levelMonsterCharacters.Length; i++)
						{
							EditorGUILayout.Space();
							EditorGUILayout.BeginVertical(arrayElementStyle);
							{
								EditorGUILayout.Space();
								globalInfo.levelMonsterCharacters[i] = (CharacterInfo)EditorGUILayout.ObjectField("LevelMonsterSkin:", globalInfo.levelMonsterCharacters[i], typeof(CharacterInfo), false);
								if (GUILayout.Button("", "PaneOptions"))
								{
									PaneOptions<CharacterInfo>(globalInfo.levelMonsterCharacters, globalInfo.levelMonsterCharacters[i], delegate (CharacterInfo[] newElement) { globalInfo.levelMonsterCharacters = newElement; });
								}
								EditorGUILayout.Space();
							}
							EditorGUILayout.EndVertical();
						}
						EditorGUI.indentLevel -= 1;
					}
					EditorGUILayout.EndVertical();
				}
				if (StyledButton("New LevelMonsterSkin"))
					globalInfo.levelMonsterCharacters = AddElement<CharacterInfo>(globalInfo.levelMonsterCharacters, null);
			}
			EditorGUILayout.EndVertical();

			// meleeWeapons
			EditorGUILayout.BeginVertical(rootGroupStyle);
			{
				EditorGUILayout.BeginHorizontal();
				{
					meleeWeaponsOptions = EditorGUILayout.Foldout(meleeWeaponsOptions, "MeleeWeapons (" + globalInfo.meleeWeapons.Length + ")", EditorStyles.foldout);
				}
				EditorGUILayout.EndHorizontal();

				if (meleeWeaponsOptions)
				{
					EditorGUILayout.BeginVertical(subGroupStyle);
					{
						EditorGUI.indentLevel += 1;
						for (int i = 0; i < globalInfo.meleeWeapons.Length; i++)
						{
							EditorGUILayout.Space();
							EditorGUILayout.BeginVertical(arrayElementStyle);
							{
								EditorGUILayout.Space();
								globalInfo.meleeWeapons[i] = (GameObject)EditorGUILayout.ObjectField("MeleeWeapon:", globalInfo.meleeWeapons[i], typeof(GameObject), false);
								if (GUILayout.Button("", "PaneOptions"))
								{
									PaneOptions<GameObject>(globalInfo.meleeWeapons, globalInfo.meleeWeapons[i], delegate (GameObject[] newElement) { globalInfo.meleeWeapons = newElement; });
								}
								EditorGUILayout.Space();
							}
							EditorGUILayout.EndVertical();
						}
						EditorGUI.indentLevel -= 1;
					}
					EditorGUILayout.EndVertical();
				}
				if (StyledButton("New MeleeWeapon"))
					globalInfo.meleeWeapons = AddElement<GameObject>(globalInfo.meleeWeapons, null);
			}
			EditorGUILayout.EndVertical();

			// bows
			EditorGUILayout.BeginVertical(rootGroupStyle);
			{
				EditorGUILayout.BeginHorizontal();
				{
					bowsOptions = EditorGUILayout.Foldout(bowsOptions, "Bows (" + globalInfo.bows.Length + ")", EditorStyles.foldout);
				}
				EditorGUILayout.EndHorizontal();

				if (bowsOptions)
				{
					EditorGUILayout.BeginVertical(subGroupStyle);
					{
						EditorGUI.indentLevel += 1;
						for (int i = 0; i < globalInfo.bows.Length; i++)
						{
							EditorGUILayout.Space();
							EditorGUILayout.BeginVertical(arrayElementStyle);
							{
								EditorGUILayout.Space();
					 			globalInfo.bows[i] = (GameObject)EditorGUILayout.ObjectField("Bow:", globalInfo.bows[i], typeof(GameObject), false);
								if (GUILayout.Button("", "PaneOptions"))
								{
									PaneOptions<GameObject>(globalInfo.bows, globalInfo.bows[i], delegate (GameObject[] newElement) { globalInfo.bows = newElement; });
								}
								EditorGUILayout.Space();
							}
							EditorGUILayout.EndVertical();
						}
						EditorGUI.indentLevel -= 1;
					}
					EditorGUILayout.EndVertical();
				}
				if (StyledButton("New Bow"))
					globalInfo.bows = AddElement<GameObject>(globalInfo.bows, null);
			}
			EditorGUILayout.EndVertical();

			// shields
			EditorGUILayout.BeginVertical(rootGroupStyle);
			{
				EditorGUILayout.BeginHorizontal();
				{
					shieldsOptions = EditorGUILayout.Foldout(shieldsOptions, "Shields (" + globalInfo.shields.Length + ")", EditorStyles.foldout);
				}
				EditorGUILayout.EndHorizontal();

				if (shieldsOptions)
				{
					EditorGUILayout.BeginVertical(subGroupStyle);
					{
						EditorGUI.indentLevel += 1;
						for (int i = 0; i < globalInfo.shields.Length; i++)
						{
							EditorGUILayout.Space();
							EditorGUILayout.BeginVertical(arrayElementStyle);
							{
								EditorGUILayout.Space();
								globalInfo.shields[i] = (GameObject)EditorGUILayout.ObjectField("Shield:", globalInfo.shields[i], typeof(GameObject), false);
								if (GUILayout.Button("", "PaneOptions"))
								{
									PaneOptions<GameObject>(globalInfo.shields, globalInfo.shields[i], delegate (GameObject[] newElement) { globalInfo.shields = newElement; });
								}
								EditorGUILayout.Space();
							}
							EditorGUILayout.EndVertical();
						}
						EditorGUI.indentLevel -= 1;
					}
					EditorGUILayout.EndVertical();
				}
				if (StyledButton("New Shield"))
					globalInfo.shields = AddElement<GameObject>(globalInfo.shields, null);
			}
			EditorGUILayout.EndVertical();

            // Round Options
            EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					roundOptions = EditorGUILayout.Foldout(roundOptions, "Round Options", foldStyle);
					helpButton("global:round");
				}EditorGUILayout.EndHorizontal();

				if (roundOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUILayout.Space();
						EditorGUI.indentLevel += 1;
						
						EditorGUIUtility.labelWidth = 210;
						//globalInfo.roundOptions.totalRounds = EditorGUILayout.IntField("Total Rounds (Best of):", globalInfo.roundOptions.totalRounds);
			
                        globalInfo.roundOptions.lvUpEffect = (GameObject)EditorGUILayout.ObjectField("LvUp Effect:", globalInfo.roundOptions.lvUpEffect, typeof(UnityEngine.GameObject), true);
                        globalInfo.roundOptions.showWaitLoadingEffect = (GameObject)EditorGUILayout.ObjectField("WaitLoadingEffect:", globalInfo.roundOptions.showWaitLoadingEffect, typeof(UnityEngine.GameObject), true);
                        globalInfo.roundOptions.coin = (GameObject)EditorGUILayout.ObjectField("coin:", globalInfo.roundOptions.coin, typeof(UnityEngine.GameObject), true);
                        globalInfo.roundOptions.coins = (GameObject)EditorGUILayout.ObjectField("coins:", globalInfo.roundOptions.coins, typeof(UnityEngine.GameObject), true);
                        //globalInfo.roundOptions.boom = (GameObject)EditorGUILayout.ObjectField("Boom:", globalInfo.roundOptions.boom, typeof(UnityEngine.GameObject), true);
                        //globalInfo.roundOptions.knife = (GameObject)EditorGUILayout.ObjectField("Knife:", globalInfo.roundOptions.knife, typeof(UnityEngine.GameObject), true);
                        
                        //globalInfo.roundOptions.archerPiece = (GameObject)EditorGUILayout.ObjectField("ArcherPiece:", globalInfo.roundOptions.archerPiece, typeof(UnityEngine.GameObject), true);
                        globalInfo.roundOptions.potionOfSpeed = (GameObject)EditorGUILayout.ObjectField("SpeedShoe:", globalInfo.roundOptions.potionOfSpeed, typeof(UnityEngine.GameObject), true);
                        globalInfo.roundOptions.potionOfHealth = (GameObject)EditorGUILayout.ObjectField("potionOfHealth:", globalInfo.roundOptions.potionOfHealth, typeof(UnityEngine.GameObject), true);
                        //globalInfo.roundOptions.potionOfAnger = (GameObject)EditorGUILayout.ObjectField("potionOfAnger:", globalInfo.roundOptions.potionOfAnger, typeof(UnityEngine.GameObject), true);
                        globalInfo.roundOptions.itemEffect = (GameObject)EditorGUILayout.ObjectField("ItemEffect:", globalInfo.roundOptions.itemEffect, typeof(UnityEngine.GameObject), true);
                        globalInfo.roundOptions.getCoinEffect = (GameObject)EditorGUILayout.ObjectField("GetCoinEffect:", globalInfo.roundOptions.getCoinEffect, typeof(UnityEngine.GameObject), true);
                       
                        //globalInfo.roundOptions.angerEffect = (GameObject)EditorGUILayout.ObjectField("AngerEffect:", globalInfo.roundOptions.angerEffect, typeof(UnityEngine.GameObject), true);
                        globalInfo.roundOptions.healthEffect = (GameObject)EditorGUILayout.ObjectField("HealthEffect:", globalInfo.roundOptions.healthEffect, typeof(UnityEngine.GameObject), true);
                        globalInfo.roundOptions.decelerateSpeedEffect = (GameObject)EditorGUILayout.ObjectField("DecelerateSpeedEffect:", globalInfo.roundOptions.decelerateSpeedEffect, typeof(UnityEngine.GameObject), true);
                        globalInfo.roundOptions.bloodBar = (GameObject)EditorGUILayout.ObjectField("BloodBar:", globalInfo.roundOptions.bloodBar, typeof(UnityEngine.GameObject), true);

                        globalInfo.roundOptions.fastEffect = (GameObject)EditorGUILayout.ObjectField("FastEffect:", globalInfo.roundOptions.fastEffect, typeof(UnityEngine.GameObject), true);

                        globalInfo.roundOptions.callBossItem = (GameObject)EditorGUILayout.ObjectField("CallBossItem:", globalInfo.roundOptions.callBossItem, typeof(UnityEngine.GameObject), true);
                        globalInfo.roundOptions.callbanditsItem = (GameObject)EditorGUILayout.ObjectField("CallbanditsItem:", globalInfo.roundOptions.callbanditsItem, typeof(UnityEngine.GameObject), true);

                        globalInfo.roundOptions.shadow = (GameObject)EditorGUILayout.ObjectField("Shadow:", globalInfo.roundOptions.shadow, typeof(UnityEngine.GameObject), true);
                        EditorGUIUtility.labelWidth = 150;
						
						EditorGUI.indentLevel -= 1;
						EditorGUILayout.Space();
					}EditorGUILayout.EndVertical();
				}
			}EditorGUILayout.EndVertical();

			// Hit Effects Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					hitOptions = EditorGUILayout.Foldout(hitOptions, "Hit Effect Options", foldStyle);
					helpButton("global:hitEffects");
				}EditorGUILayout.EndHorizontal();

				if (hitOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUILayout.Space();
						EditorGUI.indentLevel += 1;

						EditorGUIUtility.labelWidth = 200;
						HitOptionBlock("Weak Hit Options", globalInfo.hitOptions.weakHit);
						HitOptionBlock("Medium Hit Options", globalInfo.hitOptions.mediumHit);
						HitOptionBlock("Heavy Hit Options", globalInfo.hitOptions.heavyHit);
						HitOptionBlock("Crumple Hit Options", globalInfo.hitOptions.crumpleHit);

						EditorGUILayout.Space();

						HitOptionBlock("Custom Hit 1 Options", globalInfo.hitOptions.customHit1);
						HitOptionBlock("Custom Hit 2 Options", globalInfo.hitOptions.customHit2);
						HitOptionBlock("Custom Hit 3 Options", globalInfo.hitOptions.customHit3);

						EditorGUILayout.Space();

						//globalInfo.hitOptions.resetAnimationOnHit = EditorGUILayout.Toggle("Restart Animation on Hit", globalInfo.hitOptions.resetAnimationOnHit);
						//globalInfo.hitOptions.useHitStunDeceleration = EditorGUILayout.Toggle("Animation Deceleration Effect", globalInfo.hitOptions.useHitStunDeceleration);
						EditorGUIUtility.labelWidth = 150;
						
						EditorGUI.indentLevel -= 1;
						EditorGUILayout.Space();
					}EditorGUILayout.EndVertical();
				}
			}EditorGUILayout.EndVertical();

		}EditorGUILayout.EndScrollView();

		//if (Application.dataPath.Contains("C:/LocalAssets/UFE/UFE")){
		if (Application.dataPath.Contains("LocalAssets/UFE/UFE Package")){
			pName = EditorGUILayout.TextField("Package Name:", pName);
            bundlePack = EditorGUILayout.Toggle("Bundle", bundlePack);

            if (StyledButton("Export Package")) {
                string[] projectContent;
                if (bundlePack) {
                    projectContent = new string[] { 
                    "Assets/StreamingAssets", 
                    "Assets/UFE", 
                    "Assets/UFE Addons", 
                    "ProjectSettings/TagManager.asset", 
                    "ProjectSettings/TimeManager.asset", 
                    "ProjectSettings/InputManager.asset"};
                } else {
                    projectContent = new string[] { 
                    "Assets/StreamingAssets", 
                    "Assets/UFE", 
                    "ProjectSettings/TagManager.asset", 
                    "ProjectSettings/TimeManager.asset", 
                    "ProjectSettings/InputManager.asset"};
                }
                
                AssetDatabase.ExportPackage(projectContent, pName, ExportPackageOptions.Recurse);
            }

		}

		if (GUI.changed)
        {
			Undo.RecordObject(globalInfo, "Global Editor Modify");
			EditorUtility.SetDirty(globalInfo);
		}
	}

    public bool DisableScreenButton(BaseUI screen) {
        if (screen == null || (screenPreview != null && screenPreview.name != screen.name)) {
            return true;
        }
        return false;
    }

    public void ScreenButton(BaseUI screen) {
        if (screenPreview != null && screen != null && screenPreview.name == screen.name) {
            if (GUILayout.Button("Close", GUILayout.Width(45))) CloseGUICanvas();
        } else {
            if (GUILayout.Button("Open", GUILayout.Width(45))) OpenGUICanvas(screen);
        }
    }

	public void UpdateGUICanvas() {
		if (canvasPreview != null) {
			CanvasScaler cScaler = canvasPreview.GetComponent<CanvasScaler>();
			if (canvasPreview.GetComponent<Canvas>() == null) {
				cScaler = canvasPreview.AddComponent<CanvasScaler>();
			}
		}
	}

    public void OpenGUICanvas(BaseUI screen) {
        CloseGUICanvas();

        Selection.activeObject = screen;

        System.Reflection.Assembly assembly = typeof(UnityEditor.EditorWindow).Assembly;
        System.Type type = assembly.GetType("UnityEditor.InspectorWindow");
        EditorWindow inspectorView = EditorWindow.GetWindow(type);
        inspectorView.Focus();

    }

    public void CloseGUICanvas() {
        if (screenPreview != null) {
            Editor.DestroyImmediate(screenPreview);
            screenPreview = null;
        }
        if (canvasPreview != null) {
            Editor.DestroyImmediate(canvasPreview);
            canvasPreview = null;
        }
        if (eventSystemPreview != null) {
            Editor.DestroyImmediate(eventSystemPreview);
            eventSystemPreview = null;
        }
    }

	public bool StyledButton (string label) {
		EditorGUILayout.Space();
		GUILayoutUtility.GetRect(1, 20);
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		bool clickResult = GUILayout.Button(label, addButtonStyle);
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();
		return clickResult;
	}

	public void HitOptionBlock(string label, HitTypeOptions hit){
		HitOptionBlock(label, hit, false);
	}

	public void HitOptionBlock(string label, HitTypeOptions hit, bool disableFreezingTime){
		hit.editorToggle = EditorGUILayout.Foldout(hit.editorToggle, label, foldStyle);
		if (hit.editorToggle){
			EditorGUILayout.BeginVertical(subGroupStyle);{
				EditorGUILayout.Space();
				EditorGUI.indentLevel += 1;

				hit.hitParticle = (GameObject) EditorGUILayout.ObjectField("Particle Effect:", hit.hitParticle, typeof(UnityEngine.GameObject), true);
                hit.spawnPoint = (HitEffectSpawnPoint) EditorGUILayout.EnumPopup("Spawn Point:", hit.spawnPoint, enumStyle);
				hit.killTime = EditorGUILayout.FloatField("Effect Duration:", hit.killTime);
				
				if (disableFreezingTime){
					EditorGUI.BeginDisabledGroup(true);
                    //EditorGUILayout.TextField("Freezing Time (seconds):", "(Automatic)");
					EditorGUILayout.TextField("Animation Speed", "(Automatic)");
					EditorGUI.EndDisabledGroup();
				}else{
                    //hit.freezingTime = EditorGUILayout.FloatField("Freezing Time (seconds):", hit.freezingTime);
					hit.animationSpeed = EditorGUILayout.FloatField("Animation Speed (%):", hit.animationSpeed);
				}

                hit.moveSpeed = EditorGUILayout.FloatField("Move Speed (%):", hit.moveSpeed);
                hit.moveSpeedTime = EditorGUILayout.FloatField("MoveSpeed Time:", hit.moveSpeedTime);

                /*hit.autoHitStop = EditorGUILayout.Toggle("Auto Hit Stop", hit.autoHitStop);
                if (!hit.autoHitStop) {
                    hit.hitStop = EditorGUILayout.FloatField("Hit Stop (seconds):", hit.hitStop);
                } else {
                    hit.hitStop = hit.freezingTime;
                }*/

				hit.shakeCharacterOnHit = EditorGUILayout.Toggle("Shake Character On Hit", hit.shakeCharacterOnHit);
				hit.shakeCameraOnHit = EditorGUILayout.Toggle("Shake Camera On Hit", hit.shakeCameraOnHit);
				if (hit.shakeCharacterOnHit || hit.shakeCameraOnHit)
					hit.shakeDensity = EditorGUILayout.FloatField("Shake Density:", hit.shakeDensity);

				EditorGUI.indentLevel -= 1;
				EditorGUILayout.Space();
				
			}EditorGUILayout.EndVertical();
		}
	}
	
	public void PaneOptions<T> (T[] elements, T element, System.Action<T[]> callback){
		if (elements == null || elements.Length == 0) return;
		GenericMenu toolsMenu = new GenericMenu();
		
		if ((elements[0] != null && elements[0].Equals(element)) || (elements[0] == null && element == null) || elements.Length == 1){
			toolsMenu.AddDisabledItem(new GUIContent("Move Up"));
			toolsMenu.AddDisabledItem(new GUIContent("Move To Top"));
		}else {
			toolsMenu.AddItem(new GUIContent("Move Up"), false, delegate() {callback(MoveElement<T>(elements, element, -1));});
			toolsMenu.AddItem(new GUIContent("Move To Top"), false, delegate() {callback(MoveElement<T>(elements, element, -elements.Length));});
		}
		if ((elements[elements.Length - 1] != null && elements[elements.Length - 1].Equals(element)) || elements.Length == 1){
			toolsMenu.AddDisabledItem(new GUIContent("Move Down"));
			toolsMenu.AddDisabledItem(new GUIContent("Move To Bottom"));
		}else{
			toolsMenu.AddItem(new GUIContent("Move Down"), false, delegate() {callback(MoveElement<T>(elements, element, 1));});
			toolsMenu.AddItem(new GUIContent("Move To Bottom"), false, delegate() {callback(MoveElement<T>(elements, element, elements.Length));});
		}
		
		toolsMenu.AddSeparator("");
		
		if (element != null && element is System.ICloneable){
			toolsMenu.AddItem(new GUIContent("Copy"), false, delegate() {callback(CopyElement<T>(elements, element));});
		}else{
			toolsMenu.AddDisabledItem(new GUIContent("Copy"));
		}
		
		if (element != null && CloneObject.objCopy != null && CloneObject.objCopy.GetType() == typeof(T)){
			toolsMenu.AddItem(new GUIContent("Paste"), false, delegate() {callback(PasteElement<T>(elements, element));});
		}else{
			toolsMenu.AddDisabledItem(new GUIContent("Paste"));
		}
		
		toolsMenu.AddSeparator("");
		
		if (!(element is System.ICloneable)){
			toolsMenu.AddDisabledItem(new GUIContent("Duplicate"));
		}else{
			toolsMenu.AddItem(new GUIContent("Duplicate"), false, delegate() {callback(DuplicateElement<T>(elements, element));});
		}
		toolsMenu.AddItem(new GUIContent("Remove"), false, delegate() {callback(RemoveElement<T>(elements, element));});
		
		toolsMenu.ShowAsContext();
		EditorGUIUtility.ExitGUI();
	}
	
	public T[] RemoveElement<T> (T[] elements, T element) {
		List<T> elementsList = new List<T>(elements);
		elementsList.Remove(element);
		return elementsList.ToArray();
	}
	
	public T[] AddElement<T> (T[] elements, T element) {
		List<T> elementsList = new List<T>(elements);
		elementsList.Add(element);
		return elementsList.ToArray();
	}
	
	public T[] CopyElement<T> (T[] elements, T element) {
		CloneObject.objCopy = (object)(element as ICloneable).Clone();
		return elements;
	}
	
	public T[] PasteElement<T> (T[] elements, T element) {
		if (CloneObject.objCopy == null) return elements;
		List<T> elementsList = new List<T>(elements);
		elementsList.Insert(elementsList.IndexOf(element) + 1, (T)CloneObject.objCopy);
		CloneObject.objCopy = null;
		return elementsList.ToArray();
	}
	
	public T[] DuplicateElement<T> (T[] elements, T element) {
		List<T> elementsList = new List<T>(elements);
		elementsList.Insert(elementsList.IndexOf(element) + 1, (T)(element as ICloneable).Clone());
		return elementsList.ToArray();
	}
	
	public T[] MoveElement<T> (T[] elements, T element, int steps) {
		List<T> elementsList = new List<T>(elements);
		int newIndex = Mathf.Clamp(elementsList.IndexOf(element) + steps, 0, elements.Length - 1);
		elementsList.Remove(element);
		elementsList.Insert(newIndex, element);
		return elementsList.ToArray();
	}
}
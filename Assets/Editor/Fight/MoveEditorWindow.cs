using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

public class MoveEditorWindow : EditorWindow {
	public static MoveEditorWindow moveEditorWindow;
	public static MoveInfo sentMoveInfo;
	
	private MoveInfo moveInfo;
	private int fpsTemp;
	private Vector2 scrollPos;
	
	private GameObject characterPrefab;
	private GameObject opCharacterPrefab;
	private GameObject projectilePrefab;
	private int totalFramesTemp;
    private int totalFramesGlobal;
    private int totalFramesUnsaved;
    private int totalFramesOriginal;
	private float animFrame;
	private float currentSpeed;
	private float animTime;
	private bool animIsPlaying;
	private bool smoothPreview;
	private GlobalInfo globalInfo = null;

	private float camTime;
	private float prevCamTime;
	private float camStart;
	public Vector3 initialCamPosition;
	public Quaternion initialCamRotation;
	public float initialFieldOfView;

	
	private bool possibleStancesToggle;
	private bool buttonSequenceToggle;
	private bool buttonExecutionToggle;
	
	private bool previousMovesToggle;
	private bool nextMovesToggle;
	private bool blockableAreaToggle;
	private bool hitStunToggle;
	private bool damageToggle;
	private bool forceToggle;
	//private bool pullEnemyInToggle;
	//private bool pullSelfInToggle;
	private bool hitsToggle;
	private bool hurtBoxToggle;
	private bool bodyPartsToggle;
	private bool opTransformToggle;
	
	private bool bodyPartVisibilityOptions;
	private bool generalOptions;
	private bool animationOptions;
	private bool opOverrideOptions;
	private bool inputOptions;
	private bool moveLinksOptions;
	private bool particleEffectsOptions;
	private bool selfAppliedForceOptions;
    private bool flashOptions;
    private bool turnOptions;
    private bool slowMoOptions;
	private bool soundOptions;
	private bool callNpcOptions;
	private bool stanceOptions;
	private bool textWarningOptions;
	private bool cameraOptions;
	private bool pullInOptions;
	private bool activeFramesOptions;
	private bool invincibleFramesOptions;
	private bool armorOptions;
	private bool classificationOptions;
	private bool playerConditions;
	private bool projectileOptions;

	
	private bool characterWarning;
	private string errorMsg;
	
	private string titleStyle;
	private string addButtonStyle;
	private string borderBarStyle;
	private string rootGroupStyle;
	private string subGroupStyle;
	private string arrayElementStyle;
	private string subArrayElementStyle;
	private string toggleStyle;
	private string foldStyle;
	private string enumStyle;
	private string fillBarStyle1;
	private string fillBarStyle2;
	private string fillBarStyle3;
	private string fillBarStyle4;
	private string fillBarStyle5;
	private GUIStyle labelStyle;
	private GameObject emulatedPlayer;
	private GameObject emulatedCamera;

	
	[MenuItem("Window/Move Editor")]
	[CanEditMultipleObjects]
	public static void Init(){
		moveEditorWindow = EditorWindow.GetWindow<MoveEditorWindow>(false, "Move", true);
		moveEditorWindow.Show();
		EditorWindow.FocusWindowIfItsOpen<SceneView>();
		Camera sceneCam = GameObject.FindObjectOfType<Camera>();
		if (sceneCam != null){
			moveEditorWindow.initialFieldOfView = sceneCam.fieldOfView;
			moveEditorWindow.initialCamPosition = sceneCam.transform.position;
			moveEditorWindow.initialCamRotation = sceneCam.transform.rotation;
		}
		moveEditorWindow.Populate();
	}
	
	void OnSelectionChange(){
		Populate();
		Repaint();
		Clear(false, false);
	}
	
	void OnEnable(){
		Populate();
	}
	
	void OnFocus(){
		Populate();
	}

	void OnDisable(){
		Clear(true, true, true, true, true);
	}

	void OnDestroy(){
		Clear(true, true, true, true, true);
	}

	void OnLostFocus(){
		//Clear(true, false);
	}
	
	void Clear(bool destroyChar, bool resetCam){
		Clear(destroyChar, resetCam, true, false);
	}
	
	void Clear(bool destroyChar, bool resetCam, bool p1, bool p2){
		Clear(destroyChar, resetCam, p1, p2, false);
	}

	void Clear(bool destroyChar, bool resetCam, bool p1, bool p2, bool projectile){
		if (moveInfo == null) return;
		if (destroyChar){
			if (p1 && characterPrefab != null) {
				Editor.DestroyImmediate(characterPrefab);
				characterPrefab = null;
				Editor.DestroyImmediate(emulatedPlayer);
				emulatedPlayer = null;
			}
			if (p2 && opCharacterPrefab != null){
				Editor.DestroyImmediate(opCharacterPrefab);
				opCharacterPrefab = null;
			}
			if (emulatedCamera != null) {
				Camera sceneCam = GameObject.FindObjectOfType<Camera>();
				if (sceneCam.transform.parent == emulatedCamera.transform) sceneCam.transform.parent = sceneCam.transform.parent.parent;
				Editor.DestroyImmediate(emulatedCamera);
				emulatedCamera = null;
			}
			
			totalFramesGlobal = 0;
		}

		if (projectile && projectilePrefab != null){
			Editor.DestroyImmediate(projectilePrefab);
			projectilePrefab = null;
		}

		if (resetCam){
			Camera sceneCam = GameObject.FindObjectOfType<Camera>();
			if (sceneCam != null){
				sceneCam.fieldOfView = initialFieldOfView;
				sceneCam.transform.position = initialCamPosition;
				sceneCam.transform.rotation = sceneCam.transform.rotation;
			}
		}
	}
	
	void helpButton(string page){
		if (GUILayout.Button("?", GUILayout.Width(18), GUILayout.Height(18))) 
			Application.OpenURL("http://www.ufe3d.com/doku.php/"+ page);
	}

	void Update(){
		if (EditorApplication.isPlayingOrWillChangePlaymode) {
			if (characterPrefab != null){
				Editor.DestroyImmediate(characterPrefab);
				characterPrefab = null;
			}
			if (opCharacterPrefab != null){
				Editor.DestroyImmediate(opCharacterPrefab);
				opCharacterPrefab = null;
			}
			if (emulatedCamera != null) {
				Editor.DestroyImmediate(emulatedCamera);
				emulatedCamera = null;
			}
			if (projectilePrefab != null) {
				Editor.DestroyImmediate(projectilePrefab);
				projectilePrefab = null;
			}
		}
	}

    void Populate() {
        this.titleContent = new GUIContent("Move", (Texture)Resources.Load("Icons/Move"));

		//initialFieldOfView = 16;
		//initialCamPosition = new Vector3(0,8,-34);
		//initialCamRotation = Quaternion.Euler(new Vector3(6,0,0));
		// Style Definitions
		titleStyle = "MeTransOffRight";
		addButtonStyle = "CN CountBadge";
		rootGroupStyle = "GroupBox";
		subGroupStyle = "ObjectFieldThumb";
		arrayElementStyle = "flow overlay box";
		subArrayElementStyle = "HelpBox";
		foldStyle = "Foldout";
		enumStyle = "MiniPopup";
		toggleStyle = "BoldToggle";
		borderBarStyle = "ProgressBarBack";
		fillBarStyle1 = "ProgressBarBar";
		fillBarStyle2 = "flow node 2 on";
		fillBarStyle3 = "flow node 4 on";
		fillBarStyle4 = "flow node 6 on";
		fillBarStyle5 = "flow node 5 on";

		labelStyle = new GUIStyle();
		labelStyle.alignment = TextAnchor.MiddleCenter;
		labelStyle.fontStyle = FontStyle.Bold;
		labelStyle.normal.textColor = Color.white;

		if (sentMoveInfo != null){
			EditorGUIUtility.PingObject( sentMoveInfo );
			Selection.activeObject = sentMoveInfo;
			sentMoveInfo = null;
		}

		if (moveInfo != null) {
			// 1.0.6 -> 1.1 Update
			if (moveInfo.possibleStates.Length > 0 && moveInfo.selfConditions.possibleMoveStates.Length == 0){
				foreach(PossibleStates possibleState in moveInfo.possibleStates){
					PossibleMoveStates pTemp = new PossibleMoveStates();
					pTemp.possibleState = possibleState;
					moveInfo.selfConditions.possibleMoveStates = AddElement<PossibleMoveStates>(moveInfo.selfConditions.possibleMoveStates, pTemp);
				}
				System.Array.Clear(moveInfo.possibleStates, 0, moveInfo.possibleStates.Length);
			}
		}

		UnityEngine.Object[] selection = Selection.GetFiltered(typeof(MoveInfo), SelectionMode.Assets);
		if (selection.Length > 0){
			if (selection[0] == null) return;
			moveInfo = (MoveInfo) selection[0];
			fpsTemp = moveInfo.fps;
			//animationSpeedTemp = moveInfo.animationSpeed;
			totalFramesTemp = moveInfo.totalFrames;
		}

		if (moveInfo != null && moveInfo.frameLinks != null && moveInfo.frameLinks.Length > 0){
			foreach(FrameLink frameLink in moveInfo.frameLinks){
				if (frameLink.linkableMoves != null && frameLink.linkableMoves.Length == 0) {
					frameLink.linkableMoves = AddElement<MoveInfo>(frameLink.linkableMoves, new MoveInfo());
				}
			}
		}
	}
	
	public void OnGUI(){
		if (moveInfo == null){
			GUILayout.BeginHorizontal("GroupBox");
            GUILayout.Label("Select a move file first\nor create a new move.", "CN EntryInfo");
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();
			if (GUILayout.Button("Create new move"))
				ScriptableObjectUtility.CreateAsset<MoveInfo> ();
			return;
		}

		//EditorGUIUtility.labelWidth = 150;
		GUIStyle fontStyle = new GUIStyle();
        //fontStyle.font = (Font)EditorGUIUtility.Load("EditorFont.TTF");
        fontStyle.font = (Font)Resources.Load("EditorFont");
		fontStyle.fontSize = 30;
		fontStyle.alignment = TextAnchor.UpperCenter;
		fontStyle.normal.textColor = Color.white;
		fontStyle.hover.textColor = Color.white;
		EditorGUILayout.BeginVertical(titleStyle);{
			EditorGUILayout.BeginHorizontal();{
				EditorGUILayout.LabelField("", (moveInfo.moveName == ""? "New Move" : moveInfo.moveName + "|" + moveInfo.description) , fontStyle, GUILayout.Height(32));
				helpButton("move:start");
			}EditorGUILayout.EndHorizontal();
		}EditorGUILayout.EndVertical();

		if (moveInfo.animationClip != null){
            totalFramesOriginal = (int)Mathf.Abs(Mathf.Floor((moveInfo.fps * moveInfo.animationClip.length) / moveInfo.animationSpeed));
            if (moveInfo.totalFrames == 0) {
                moveInfo.totalFrames = totalFramesOriginal;
            }
            if (moveInfo.totalFrames > totalFramesGlobal) totalFramesGlobal = moveInfo.totalFrames;
		}


		// Begin General Options
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);{
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					generalOptions = EditorGUILayout.Foldout(generalOptions, "General", foldStyle);
					helpButton("move:general");
				}EditorGUILayout.EndHorizontal();

				if (generalOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUILayout.Space();
						EditorGUI.indentLevel += 1;
						
						EditorGUIUtility.labelWidth = 180;

						moveInfo.moveName = EditorGUILayout.TextField("Move Name:", moveInfo.moveName);
                        moveInfo.cdTime = EditorGUILayout.FloatField("cd Time:", moveInfo.cdTime);
                        //moveInfo.castableCount = EditorGUILayout.FloatField("castable Time:", moveInfo.castableCount);
                        //moveInfo.resetCastableGapTime = EditorGUILayout.FloatField("resetCastableGap Time:", moveInfo.resetCastableGapTime);
                        moveInfo.description = EditorGUILayout.TextField("Move Description:", moveInfo.description);

                        EditorGUILayout.BeginHorizontal();{
							string unsaved = fpsTemp != moveInfo.fps ? "*":"";
							fpsTemp = EditorGUILayout.IntSlider("FPS Architecture:"+ unsaved, fpsTemp, 10, 120);
							EditorGUI.BeginDisabledGroup(fpsTemp == moveInfo.fps);
                            {
								if (StyledButton("Apply")) moveInfo.fps = fpsTemp;
							}EditorGUI.EndDisabledGroup();

						}EditorGUILayout.EndHorizontal();
						EditorGUILayout.Space();

						EditorGUI.indentLevel -= 1;

					}EditorGUILayout.EndVertical();
				}

			}EditorGUILayout.EndVertical();
			// End General Options

			// Begin Animation Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					animationOptions = EditorGUILayout.Foldout(animationOptions, "Animation", foldStyle);
					helpButton("move:animation");
				}EditorGUILayout.EndHorizontal();

				if (animationOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUILayout.Space();
						EditorGUI.indentLevel += 1;
						EditorGUIUtility.labelWidth = 200;

						SubGroupTitle("File");
						moveInfo.animationClip = (AnimationClip) EditorGUILayout.ObjectField("Animation Clip:", moveInfo.animationClip, typeof(UnityEngine.AnimationClip), true);
						if (moveInfo.animationClip != null){
							moveInfo.wrapMode = (WrapMode)EditorGUILayout.EnumPopup("Wrap Mode:", moveInfo.wrapMode, enumStyle);
							
							//moveInfo.disableHeadLook = EditorGUILayout.Toggle("Disable Head Look", moveInfo.disableHeadLook, toggleStyle);
                            //moveInfo.applyRootMotion = EditorGUILayout.Toggle("Apply Root Motion", moveInfo.applyRootMotion, toggleStyle);

                            SubGroupTitle("Speed");
                            moveInfo.fixedSpeed = EditorGUILayout.Toggle("Fixed Speed", moveInfo.fixedSpeed, toggleStyle);
                            moveInfo.animationSpeed = EditorGUILayout.FloatField("Default Speed:", moveInfo.animationSpeed);

                            if (moveInfo.fixedSpeed) {
                                totalFramesTemp = totalFramesOriginal;

                            } else {
                                moveInfo.speedKeyFrameToggle = EditorGUILayout.Foldout(moveInfo.speedKeyFrameToggle, "Speed Key Frames", EditorStyles.foldout);
                                if (moveInfo.speedKeyFrameToggle) {
                                    EditorGUILayout.BeginVertical(subGroupStyle);
                                    {
                                        EditorGUI.indentLevel += 1;

                                        List<int> castingValues = new List<int>();
                                        foreach (AnimSpeedKeyFrame animationKeyFrame in moveInfo.animSpeedKeyFrame)
                                            castingValues.Add(animationKeyFrame.castingFrame);
                                        StyledMarker("Casting Timeline", castingValues.ToArray(), totalFramesOriginal, EditorGUI.indentLevel);

                                        totalFramesTemp = 0;
                                        int nextCastingFrame = 0;
                                        int previousCastingFrame = 0;

                                        if (moveInfo.animSpeedKeyFrame.Length > 0) {
                                            totalFramesTemp = (int)Mathf.Floor(moveInfo.animSpeedKeyFrame[0].castingFrame / moveInfo.animationSpeed);
                                        }

                                        for (int i = 0; i < moveInfo.animSpeedKeyFrame.Length; i++) {
                                            EditorGUILayout.Space();
                                            EditorGUILayout.BeginVertical(arrayElementStyle);
                                            {
                                                EditorGUILayout.Space();
                                                EditorGUILayout.BeginHorizontal();
                                                {
                                                    moveInfo.animSpeedKeyFrame[i].castingFrame = EditorGUILayout.IntSlider("Casting Frame:", moveInfo.animSpeedKeyFrame[i].castingFrame, 0, totalFramesOriginal);
                                                    if (GUILayout.Button("", "PaneOptions")) {
                                                        PaneOptions<AnimSpeedKeyFrame>(moveInfo.animSpeedKeyFrame, moveInfo.animSpeedKeyFrame[i], delegate(AnimSpeedKeyFrame[] newElement) { moveInfo.animSpeedKeyFrame = newElement; });
                                                    }
                                                } EditorGUILayout.EndHorizontal();
                                                moveInfo.animSpeedKeyFrame[i].speed = EditorGUILayout.FloatField("New Speed:", moveInfo.animSpeedKeyFrame[i].speed);

                                                moveInfo.animSpeedKeyFrame[i].castingFrame = Mathf.Max(moveInfo.animSpeedKeyFrame[i].castingFrame, previousCastingFrame);
                                                moveInfo.animSpeedKeyFrame[i].castingFrame = Mathf.Min(moveInfo.animSpeedKeyFrame[i].castingFrame, totalFramesOriginal);

                                                nextCastingFrame = (i == moveInfo.animSpeedKeyFrame.Length - 1) ? totalFramesOriginal : moveInfo.animSpeedKeyFrame[i + 1].castingFrame;
                                                float frameWindow = Mathf.Max(0, nextCastingFrame - moveInfo.animSpeedKeyFrame[i].castingFrame);
                                                
                                                totalFramesTemp += (int)Mathf.Floor(frameWindow / moveInfo.animSpeedKeyFrame[i].speed);
                                                previousCastingFrame = moveInfo.animSpeedKeyFrame[i].castingFrame;

                                                // Update Display Value
                                                nextCastingFrame = (i == moveInfo.animSpeedKeyFrame.Length - 1) ? totalFramesUnsaved : moveInfo.animSpeedKeyFrame[i + 1].castingFrame;
                                                frameWindow = Mathf.Max(0, nextCastingFrame - moveInfo.animSpeedKeyFrame[i].castingFrame);
                                                EditorGUILayout.LabelField("Frame Window (Aprox.):", frameWindow.ToString());

                                                EditorGUILayout.Space();

                                            } EditorGUILayout.EndVertical();
                                            EditorGUILayout.Space();
                                        }
                                        if (totalFramesTemp == 0) totalFramesTemp = moveInfo.totalFrames;
                                        totalFramesUnsaved = totalFramesTemp;

                                        if (StyledButton("New Keyframe"))
                                            moveInfo.animSpeedKeyFrame = AddElement<AnimSpeedKeyFrame>(moveInfo.animSpeedKeyFrame, new AnimSpeedKeyFrame());

                                        EditorGUILayout.Space();
                                        EditorGUI.indentLevel -= 1;

                                    } EditorGUILayout.EndVertical();
                                }
                            }


                            //animationSpeedTemp = StyledSlider("Animation Speed" + unsaved, animationSpeedTemp, EditorGUI.indentLevel, -5, 5);
                            EditorGUILayout.BeginHorizontal();
                            {
                                string unsaved = totalFramesTemp != moveInfo.totalFrames ? "*" : "";
                                EditorGUILayout.LabelField("Total frames:", totalFramesTemp.ToString() + unsaved);
                                if (StyledButton("Apply")) {
                                    moveInfo.totalFrames = totalFramesTemp;
                                }
                            } EditorGUILayout.EndHorizontal();
                            EditorGUILayout.Space();


							SubGroupTitle("Blending");
							moveInfo.overrideBlendingIn = EditorGUILayout.Toggle("Override Blending (In)", moveInfo.overrideBlendingIn, toggleStyle);
							if (moveInfo.overrideBlendingIn){
								moveInfo.blendingIn = EditorGUILayout.FloatField("Blend In Duration:", moveInfo.blendingIn);
							}
							
							moveInfo.overrideBlendingOut = EditorGUILayout.Toggle("Override Blending (Out)", moveInfo.overrideBlendingOut, toggleStyle);
							if (moveInfo.overrideBlendingOut){
								moveInfo.blendingOut = EditorGUILayout.FloatField("Blend Out Duration:", moveInfo.blendingOut);
							}
							EditorGUILayout.Space();

                            SubGroupTitle("Preview");
							EditorGUIUtility.labelWidth = 180;
							GameObject newCharacterPrefab = (GameObject) EditorGUILayout.ObjectField("Character Prefab:", moveInfo.characterPrefab, typeof(UnityEngine.GameObject), true);
							if (newCharacterPrefab != null && moveInfo.characterPrefab != newCharacterPrefab && !EditorApplication.isPlayingOrWillChangePlaymode){
								if (PrefabUtility.GetPrefabType(newCharacterPrefab) != PrefabType.Prefab){
									characterWarning = true;
									errorMsg = "This character is not a prefab.";
								}else if (newCharacterPrefab.GetComponent<HitBoxesScript>() == null){
									characterWarning = true;
									errorMsg = "This character doesn't have hitboxes!\n Please add the HitboxScript and try again.";
								}else{
									characterWarning = false;
									moveInfo.characterPrefab = newCharacterPrefab;
								}
							}else if (moveInfo.characterPrefab != newCharacterPrefab && EditorApplication.isPlayingOrWillChangePlaymode){
								characterWarning = true;
								errorMsg = "You can't change this field while in play mode.";
							}else if (newCharacterPrefab == null) moveInfo.characterPrefab = null;

							if (characterPrefab == null){
								if (StyledButton("Animation Preview")){
									if (moveInfo.characterPrefab == null) {
										characterWarning = true;
										errorMsg = "Drag a character into 'Character Prefab' first.";
									}else if (EditorApplication.isPlayingOrWillChangePlaymode){
										characterWarning = true;
										errorMsg = "You can't preview animations while in play mode.";
									}else{
										characterWarning = false;
										EditorCamera.SetPosition(Vector3.up * 4);
										EditorCamera.SetRotation(Quaternion.identity);
										EditorCamera.SetOrthographic(true);
										EditorCamera.SetSize(10);

										characterPrefab = (GameObject) PrefabUtility.InstantiatePrefab(moveInfo.characterPrefab);
										characterPrefab.transform.position = new Vector3(0,0,0);
									}
								}

								if (characterWarning){
									GUILayout.BeginHorizontal("GroupBox");
									GUILayout.FlexibleSpace();
									GUILayout.Label(errorMsg,"CN EntryWarn");
									GUILayout.FlexibleSpace();
									GUILayout.EndHorizontal();
								}
							}else {
								EditorGUI.indentLevel += 1;
								if (smoothPreview){
									animFrame = StyledSlider("Animation Frames", animFrame, EditorGUI.indentLevel, 0, totalFramesGlobal);
								}else{
									animFrame = StyledSlider("Animation Frames", (int)animFrame, EditorGUI.indentLevel, 0, totalFramesGlobal);
								}
								EditorGUI.indentLevel -= 1;
								
								if (cameraOptions){
									GUILayout.BeginHorizontal("GroupBox");
									GUILayout.Label("You must close 'Cinematic Options' first.","CN EntryError");
									GUILayout.EndHorizontal();
								}

								smoothPreview = EditorGUILayout.Toggle("Smooth Preview", smoothPreview, toggleStyle);
								AnimationSampler(characterPrefab, moveInfo.animationClip, 0, true, true, false, false);

                                EditorGUILayout.LabelField("Current Speed:", currentSpeed.ToString());

								EditorGUILayout.Space();
								
								EditorGUILayout.BeginHorizontal();{
									if (StyledButton("Reset Scene View")){
										EditorCamera.SetPosition(Vector3.up * 4);
										EditorCamera.SetRotation(Quaternion.identity);
										EditorCamera.SetOrthographic(true);
										EditorCamera.SetSize(10);
									}
									if (StyledButton("Close Preview")) Clear (true, true);
								}EditorGUILayout.EndHorizontal();
								
								EditorGUILayout.Space();
							}
						}
						EditorGUI.indentLevel -= 1;
						EditorGUIUtility.labelWidth = 150;

					}EditorGUILayout.EndVertical();
				}else if (characterPrefab != null && !cameraOptions){
					Clear (true, true);
				}
				
			}EditorGUILayout.EndVertical();
			// End Animation Options


			// Begin Active Frame Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					activeFramesOptions = EditorGUILayout.Foldout(activeFramesOptions, "Active Frames", EditorStyles.foldout);
					helpButton("move:activeframes");
				}EditorGUILayout.EndHorizontal();
				
				if (activeFramesOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUI.indentLevel += 1;
						
						// Hits Toggle
						hitsToggle = EditorGUILayout.Foldout(hitsToggle, "Hits ("+ moveInfo.hits.Length +")", EditorStyles.foldout);
						if (hitsToggle){
							EditorGUILayout.BeginVertical(subGroupStyle);{
								EditorGUI.indentLevel += 1;
								List<Vector3> castingValues = new List<Vector3>();
								foreach(Hit hit in moveInfo.hits) {
									castingValues.Add(new Vector3(hit.activeFramesBegin, hit.activeFramesEnds, (0)));
								}
								StyledMarker("Frame Data Timeline", castingValues.ToArray(), moveInfo.totalFrames, 
								             EditorGUI.indentLevel, true);
								
								for (int i = 0; i < moveInfo.hits.Length; i ++){
									EditorGUILayout.Space();
									EditorGUILayout.BeginVertical(arrayElementStyle);{
										EditorGUILayout.Space();
										EditorGUILayout.BeginHorizontal();{
											StyledMinMaxSlider("Active Frames", ref moveInfo.hits[i].activeFramesBegin, ref moveInfo.hits[i].activeFramesEnds, 0, moveInfo.totalFrames, EditorGUI.indentLevel);
											if (GUILayout.Button("", "PaneOptions")){
												PaneOptions<Hit>(moveInfo.hits, moveInfo.hits[i], delegate (Hit[] newElement) { moveInfo.hits = newElement; });
											}
										}EditorGUILayout.EndHorizontal();
										
										EditorGUIUtility.labelWidth = 180;
										
										EditorGUILayout.Space();

										// Hurt Boxes Toggle
										int amount = moveInfo.hits[i].hurtBoxes != null ? moveInfo.hits[i].hurtBoxes.Length : 0;
										moveInfo.hits[i].hurtBoxesToggle = EditorGUILayout.Foldout(moveInfo.hits[i].hurtBoxesToggle, "Hurt Boxes ("+ amount +")", EditorStyles.foldout);
										if (moveInfo.hits[i].hurtBoxesToggle){
											EditorGUILayout.BeginVertical(subGroupStyle);{
												EditorGUI.indentLevel += 1;
												if (amount > 0){
													for (int y = 0; y < moveInfo.hits[i].hurtBoxes.Length; y ++){
														EditorGUILayout.BeginVertical(subArrayElementStyle);{
															EditorGUILayout.BeginHorizontal();{
																moveInfo.hits[i].hurtBoxes[y].bodyPart = (BodyPart)EditorGUILayout.EnumPopup("Body Part:", moveInfo.hits[i].hurtBoxes[y].bodyPart, enumStyle);
																if (GUILayout.Button("", "PaneOptions")){
																	PaneOptions<HurtBox>(moveInfo.hits[i].hurtBoxes, moveInfo.hits[i].hurtBoxes[y], delegate (HurtBox[] newElement) { moveInfo.hits[i].hurtBoxes = newElement; });
																}
															}EditorGUILayout.EndHorizontal();
															
															moveInfo.hits[i].hurtBoxes[y].shape = (HitBoxShape) EditorGUILayout.EnumPopup("Shape:", moveInfo.hits[i].hurtBoxes[y].shape, enumStyle);
															if (moveInfo.hits[i].hurtBoxes[y].shape == HitBoxShape.circle){
																moveInfo.hits[i].hurtBoxes[y].radius = EditorGUILayout.FloatField("Radius:", moveInfo.hits[i].hurtBoxes[y].radius);
                                                            }
															EditorGUILayout.Space();
														}EditorGUILayout.EndVertical();
													}
												}
												if (StyledButton("New Hurt Box"))
													moveInfo.hits[i].hurtBoxes = AddElement<HurtBox>(moveInfo.hits[i].hurtBoxes, new HurtBox());
												
												EditorGUI.indentLevel -= 1;
											}EditorGUILayout.EndVertical();
										}

                                        EditorGUILayout.Space();

										// Damage Toggle
										moveInfo.hits[i].damageOptionsToggle = EditorGUILayout.Foldout(moveInfo.hits[i].damageOptionsToggle, "Damage Options", EditorStyles.foldout);
										if (moveInfo.hits[i].damageOptionsToggle){
											EditorGUILayout.BeginVertical(subGroupStyle);{
												EditorGUIUtility.labelWidth = 180;
												EditorGUI.indentLevel += 1;
												moveInfo.hits[i].damageType = (DamageType) EditorGUILayout.EnumPopup("Damage Type:", moveInfo.hits[i].damageType, enumStyle);
												moveInfo.hits[i].damageOnHit = EditorGUILayout.FloatField("Damage on Hit:", moveInfo.hits[i].damageOnHit);
												//moveInfo.hits[i].damageOnBlock = EditorGUILayout.FloatField("Damage on Block:", moveInfo.hits[i].damageOnBlock);
												//moveInfo.hits[i].damageScaling = EditorGUILayout.Toggle("Damage Scaling", moveInfo.hits[i].damageScaling, toggleStyle);
												moveInfo.hits[i].doesntKill = EditorGUILayout.Toggle("Hit Doesn't Kill", moveInfo.hits[i].doesntKill, toggleStyle);
												EditorGUI.indentLevel -= 1;
												EditorGUIUtility.labelWidth = 150;
											}EditorGUILayout.EndVertical();
										}

										// Force Toggle
										moveInfo.hits[i].forceOptionsToggle = EditorGUILayout.Foldout(moveInfo.hits[i].forceOptionsToggle, "Force Options", EditorStyles.foldout);
										if (moveInfo.hits[i].forceOptionsToggle){
											EditorGUILayout.BeginVertical(subGroupStyle);{
												EditorGUI.indentLevel += 1;
												EditorGUIUtility.labelWidth = 220;
												moveInfo.hits[i].opponentForceToggle = EditorGUILayout.Foldout(moveInfo.hits[i].opponentForceToggle, "Opponent", EditorStyles.foldout);
												if (moveInfo.hits[i].opponentForceToggle) {
													EditorGUI.indentLevel += 1;
													moveInfo.hits[i].pushForce = EditorGUILayout.Vector2Field("Applied Force", moveInfo.hits[i].pushForce);
													EditorGUI.indentLevel -= 1;
												}
												EditorGUIUtility.labelWidth = 150;
												EditorGUI.indentLevel -= 1;
											}EditorGUILayout.EndVertical();
										}

                                        // Override Events Toggle
                                        moveInfo.hits[i].overrideEventsToggle = EditorGUILayout.Foldout(moveInfo.hits[i].overrideEventsToggle, "Override Events (On Hit)", EditorStyles.foldout);
                                        if (moveInfo.hits[i].overrideEventsToggle)
                                        {
                                            EditorGUILayout.BeginVertical(subGroupStyle);
                                            {
                                                EditorGUI.indentLevel += 1;
                                                moveInfo.hits[i].hitStrength = (HitStrengh)EditorGUILayout.EnumPopup("Hit Strength:", moveInfo.hits[i].hitStrength, enumStyle);
                                                EditorGUIUtility.labelWidth = 240;
                                                moveInfo.hits[i].overrideHitEffects = EditorGUILayout.Toggle("Override Hit Effects", moveInfo.hits[i].overrideHitEffects, toggleStyle);
                                                EditorGUIUtility.labelWidth = 210;
                                                if (moveInfo.hits[i].overrideHitEffects) HitOptionBlock("Hit Effects", moveInfo.hits[i].hitEffects);
                                                EditorGUIUtility.labelWidth = 240;

                                                moveInfo.hits[i].overrideHitAnimation = EditorGUILayout.Toggle("Override Hit Animation", moveInfo.hits[i].overrideHitAnimation, toggleStyle);
                                                EditorGUIUtility.labelWidth = 210;
                                                if (moveInfo.hits[i].overrideHitAnimation) moveInfo.hits[i].newHitAnimation = (BasicMoveReference)EditorGUILayout.EnumPopup("- Hit Animation:", moveInfo.hits[i].newHitAnimation, enumStyle);
                                                EditorGUIUtility.labelWidth = 240;

                                                moveInfo.hits[i].overrideEffectSpawnPoint = EditorGUILayout.Toggle("Override Effect Spawn Point", moveInfo.hits[i].overrideEffectSpawnPoint, toggleStyle);
                                                EditorGUIUtility.labelWidth = 210;
                                                if (moveInfo.hits[i].overrideEffectSpawnPoint) moveInfo.hits[i].spawnPoint = (HitEffectSpawnPoint)EditorGUILayout.EnumPopup("- Spawn Point:", moveInfo.hits[i].spawnPoint, enumStyle);
                                                EditorGUIUtility.labelWidth = 240;

                                                moveInfo.hits[i].overrideHitAnimationBlend = EditorGUILayout.Toggle("Override Hit Animation Blend-in", moveInfo.hits[i].overrideHitAnimationBlend, toggleStyle);
                                                EditorGUIUtility.labelWidth = 210;
                                                if (moveInfo.hits[i].overrideHitAnimationBlend) EditorGUILayout.FloatField("- Blending In:", moveInfo.hits[i].newHitBlendingIn);
                                                EditorGUIUtility.labelWidth = 240;
                                                EditorGUI.indentLevel -= 1;
                                            }
                                            EditorGUILayout.EndVertical();
                                        }
                                        
										EditorGUILayout.Space();
									}EditorGUILayout.EndVertical();
								}
								
								if (StyledButton("New Hit"))
									moveInfo.hits = AddElement<Hit>(moveInfo.hits, new Hit());
								
								EditorGUI.indentLevel -= 1;
							}EditorGUILayout.EndVertical();
						}
						
					}EditorGUILayout.EndVertical();
				}
				
			}EditorGUILayout.EndVertical();
			// End Active Frame Options
			
			// Player Conditions
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					playerConditions = EditorGUILayout.Foldout(playerConditions, "Player Conditions", EditorStyles.foldout);
					helpButton("move:playerconditions");
				}EditorGUILayout.EndHorizontal();
				
				if (playerConditions){
					PlayerConditionsGroup("Self", moveInfo.selfConditions, true);
					//PlayerConditionsGroup("Opponent", moveInfo.opponentConditions, true);
				}
			}EditorGUILayout.EndVertical();
			// End Player Conditions


			// Begin Input Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					inputOptions = EditorGUILayout.Foldout(inputOptions, "Input", EditorStyles.foldout);
					helpButton("move:input");
				}EditorGUILayout.EndHorizontal();

				if (inputOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
                        EditorGUI.indentLevel += 1;					
    					// Button Execution
						buttonExecutionToggle = EditorGUILayout.Foldout(buttonExecutionToggle, "Button Executions ("+ moveInfo.buttonExecution.Length +")", EditorStyles.foldout);
						if (buttonExecutionToggle){
							EditorGUILayout.BeginVertical(subGroupStyle);{
								EditorGUI.indentLevel += 1;
								//moveInfo.onPressExecution = EditorGUILayout.Toggle("On Button Press", moveInfo.onPressExecution, toggleStyle);
								//moveInfo.onReleaseExecution = EditorGUILayout.Toggle("On Button Release", moveInfo.onReleaseExecution, toggleStyle);
								for (int i = 0; i < moveInfo.buttonExecution.Length; i ++){
									EditorGUILayout.Space();
									EditorGUILayout.BeginVertical(arrayElementStyle);{
										EditorGUILayout.Space();
										EditorGUILayout.BeginHorizontal();{
											moveInfo.buttonExecution[i] = (ButtonPress)EditorGUILayout.EnumPopup("Button:", moveInfo.buttonExecution[i], enumStyle);
											if (GUILayout.Button("", "PaneOptions")){
												PaneOptions<ButtonPress>(moveInfo.buttonExecution, moveInfo.buttonExecution[i], delegate (ButtonPress[] newElement) { moveInfo.buttonExecution = newElement; });
											}
										}EditorGUILayout.EndHorizontal();
										EditorGUILayout.Space();
									}EditorGUILayout.EndHorizontal();
								}
								EditorGUILayout.Space();
								if (StyledButton("New Button Execution"))
									moveInfo.buttonExecution = AddElement<ButtonPress>(moveInfo.buttonExecution, ButtonPress.Button1);

								EditorGUI.indentLevel -= 1;
							}EditorGUILayout.EndVertical();
                        }
						EditorGUI.indentLevel -= 1;
					}EditorGUILayout.EndVertical();
				}
				
			}EditorGUILayout.EndVertical();
			// End Input Options

#if !UFE_BASIC
			// Begin Move Link Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					moveLinksOptions = EditorGUILayout.Foldout(moveLinksOptions, "Chain Moves", EditorStyles.foldout);
					helpButton("move:chainmoves");
				}EditorGUILayout.EndHorizontal();

				if (moveLinksOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUI.indentLevel += 1;
						
						// Previous Moves
						previousMovesToggle = EditorGUILayout.Foldout(previousMovesToggle, "Required Moves (" + moveInfo.previousMoves.Length + ")", EditorStyles.foldout);
						if (previousMovesToggle){
							EditorGUILayout.BeginVertical(subGroupStyle);{
								EditorGUI.indentLevel += 1;
								for (int i = 0; i < moveInfo.previousMoves.Length; i ++){
									EditorGUILayout.Space();
									EditorGUILayout.BeginVertical(arrayElementStyle);{
										EditorGUILayout.Space();
										EditorGUILayout.BeginHorizontal();{
											moveInfo.previousMoves[i] = (MoveInfo) EditorGUILayout.ObjectField("Move:", moveInfo.previousMoves[i], typeof(MoveInfo), false);
											if (GUILayout.Button("", "PaneOptions")){
												PaneOptions<MoveInfo>(moveInfo.previousMoves, moveInfo.previousMoves[i], delegate (MoveInfo[] newElement) { moveInfo.previousMoves = newElement; });
											}
										}EditorGUILayout.EndHorizontal();
										EditorGUILayout.Space();
									}EditorGUILayout.EndHorizontal();
								}
								EditorGUILayout.Space();
								if (StyledButton("New Required Move"))
									moveInfo.previousMoves = AddElement<MoveInfo>(moveInfo.previousMoves, null);
								
								EditorGUILayout.Space();
								EditorGUI.indentLevel -= 1;

							}EditorGUILayout.EndVertical();
						}
						
						
						// Move Links
						nextMovesToggle = EditorGUILayout.Foldout(nextMovesToggle, "Move Links ("+ moveInfo.frameLinks.Length +")", EditorStyles.foldout);
						if (nextMovesToggle){
							EditorGUILayout.BeginVertical(subGroupStyle);{
								EditorGUI.indentLevel += 1;

								for (int i = 0; i < moveInfo.frameLinks.Length; i ++){
									
									EditorGUILayout.Space();
									EditorGUILayout.BeginVertical(arrayElementStyle);{
										EditorGUILayout.Space();
										EditorGUILayout.BeginHorizontal();{
											StyledMinMaxSlider("Frame Links", ref moveInfo.frameLinks[i].activeFramesBegins, ref moveInfo.frameLinks[i].activeFramesEnds, 0, moveInfo.totalFrames, EditorGUI.indentLevel);
											if (GUILayout.Button("", "PaneOptions")){
												PaneOptions<FrameLink>(moveInfo.frameLinks, moveInfo.frameLinks[i], delegate (FrameLink[] newElement) { moveInfo.frameLinks = newElement; });
											}
										}EditorGUILayout.EndHorizontal();
										EditorGUILayout.Space();
										EditorGUILayout.Space();
										
										
										moveInfo.frameLinks[i].linkType = (LinkType) EditorGUILayout.EnumPopup("Link Conditions:", moveInfo.frameLinks[i].linkType, enumStyle);

										if (moveInfo.frameLinks[i].linkType == LinkType.HitConfirm){
											moveInfo.frameLinks[i].hitConfirmToggle = EditorGUILayout.Foldout(moveInfo.frameLinks[i].hitConfirmToggle, "Hit Confirm Options", EditorStyles.foldout);
											if (moveInfo.frameLinks[i].hitConfirmToggle){
												EditorGUI.indentLevel += 1;

												EditorGUILayout.BeginVertical(subGroupStyle);{
													EditorGUIUtility.labelWidth = 180;
													moveInfo.frameLinks[i].onStrike = EditorGUILayout.Toggle("On Strike", moveInfo.frameLinks[i].onStrike, toggleStyle);
													moveInfo.frameLinks[i].onBlock = EditorGUILayout.Toggle("On Block", moveInfo.frameLinks[i].onBlock, toggleStyle);
													moveInfo.frameLinks[i].onParry = EditorGUILayout.Toggle("On Parry", moveInfo.frameLinks[i].onParry, toggleStyle);
													EditorGUIUtility.labelWidth = 150;

												}EditorGUILayout.EndVertical();
												EditorGUI.indentLevel -= 1;
											}
										}else if (moveInfo.frameLinks[i].linkType == LinkType.CounterMove){
											moveInfo.frameLinks[i].counterMoveToggle = EditorGUILayout.Foldout(moveInfo.frameLinks[i].counterMoveToggle, "Counter Move Options", EditorStyles.foldout);
											if (moveInfo.frameLinks[i].counterMoveToggle){
												EditorGUI.indentLevel += 1;
												EditorGUIUtility.labelWidth = 180;
												EditorGUILayout.BeginVertical(subGroupStyle);{
													moveInfo.frameLinks[i].counterMoveType = (CounterMoveType) EditorGUILayout.EnumPopup("Filter Type:", moveInfo.frameLinks[i].counterMoveType, enumStyle);

													if (moveInfo.frameLinks[i].counterMoveType == CounterMoveType.MoveFilter){
														moveInfo.frameLinks[i].anyHitStrength = EditorGUILayout.Toggle("Any Hit Strength", moveInfo.frameLinks[i].anyHitStrength, toggleStyle);
														EditorGUI.BeginDisabledGroup(moveInfo.frameLinks[i].anyHitStrength);{
															moveInfo.frameLinks[i].hitStrength = (HitStrengh) EditorGUILayout.EnumPopup("Hit Strength:", moveInfo.frameLinks[i].hitStrength, enumStyle);
														}EditorGUI.EndDisabledGroup();
														
														moveInfo.frameLinks[i].anyStrokeHitBox = EditorGUILayout.Toggle("Any Stroke Hit Box", moveInfo.frameLinks[i].anyStrokeHitBox, toggleStyle);
														EditorGUI.BeginDisabledGroup(moveInfo.frameLinks[i].anyStrokeHitBox);{
															moveInfo.frameLinks[i].hitBoxType = (HitBoxType) EditorGUILayout.EnumPopup("Stroke Hit Box:", moveInfo.frameLinks[i].hitBoxType, enumStyle);
														}EditorGUI.EndDisabledGroup();
														
														moveInfo.frameLinks[i].anyHitType = EditorGUILayout.Toggle("Any Hit Type", moveInfo.frameLinks[i].anyHitType, toggleStyle);
														EditorGUI.BeginDisabledGroup(moveInfo.frameLinks[i].anyHitType);{
															moveInfo.frameLinks[i].hitType = (HitType) EditorGUILayout.EnumPopup("Hit Type:", moveInfo.frameLinks[i].hitType, enumStyle);
														}EditorGUI.EndDisabledGroup();
													}else{
														moveInfo.frameLinks[i].counterMoveFilter = (MoveInfo) EditorGUILayout.ObjectField("Move:", moveInfo.frameLinks[i].counterMoveFilter, typeof(MoveInfo), false);
													}

													moveInfo.frameLinks[i].disableHitImpact = EditorGUILayout.Toggle("Disable Hit Impact", moveInfo.frameLinks[i].disableHitImpact, toggleStyle);
												}EditorGUILayout.EndVertical();

												EditorGUIUtility.labelWidth = 150;
												EditorGUI.indentLevel -= 1;
											}
										}

										moveInfo.frameLinks[i].linkableMovesToggle = EditorGUILayout.Foldout(moveInfo.frameLinks[i].linkableMovesToggle, "Linked Moves ("+ moveInfo.frameLinks[i].linkableMoves.Length +")", EditorStyles.foldout);
										if (moveInfo.frameLinks[i].linkableMovesToggle){
											EditorGUI.indentLevel += 1;
											EditorGUILayout.BeginVertical(subGroupStyle);{
												EditorGUIUtility.labelWidth = 250;
												moveInfo.frameLinks[i].ignoreInputs = EditorGUILayout.Toggle("Ignore Inputs (Auto Execution)", moveInfo.frameLinks[i].ignoreInputs, toggleStyle);
												moveInfo.frameLinks[i].ignorePlayerConditions = EditorGUILayout.Toggle("Ignore Player Conditions", moveInfo.frameLinks[i].ignorePlayerConditions, toggleStyle);
												
												EditorGUI.BeginDisabledGroup(moveInfo.frameLinks[i].ignoreInputs);{
													moveInfo.frameLinks[i].allowBuffer = EditorGUILayout.Toggle("Allow Buffer", moveInfo.frameLinks[i].allowBuffer, toggleStyle);
												}EditorGUI.EndDisabledGroup();
												
												EditorGUIUtility.labelWidth = 150;
												moveInfo.frameLinks[i].nextMoveStartupFrame = Mathf.Max(0,EditorGUILayout.IntField("Startup Frame:", moveInfo.frameLinks[i].nextMoveStartupFrame));


												for (int k = 0; k < moveInfo.frameLinks[i].linkableMoves.Length; k ++){
													EditorGUILayout.Space();
													EditorGUILayout.BeginVertical(subArrayElementStyle);{
														EditorGUILayout.Space();
														EditorGUILayout.BeginHorizontal();{
															moveInfo.frameLinks[i].linkableMoves[k] = (MoveInfo) EditorGUILayout.ObjectField("Move:", moveInfo.frameLinks[i].linkableMoves[k], typeof(MoveInfo), false);
															if (GUILayout.Button("", "PaneOptions")){
																PaneOptions<MoveInfo>(moveInfo.frameLinks[i].linkableMoves, moveInfo.frameLinks[i].linkableMoves[k], delegate (MoveInfo[] newElement) { moveInfo.frameLinks[i].linkableMoves = newElement; });
															}
														}EditorGUILayout.EndHorizontal();
														EditorGUILayout.Space();
													}EditorGUILayout.EndVertical();
												}
												EditorGUILayout.Space();

												if (StyledButton("New Move"))
													moveInfo.frameLinks[i].linkableMoves = AddElement<MoveInfo>(moveInfo.frameLinks[i].linkableMoves, null);
												
												EditorGUILayout.Space();

											}EditorGUILayout.EndVertical();

											EditorGUI.indentLevel -= 1;
											EditorGUILayout.Space();
										}

									}EditorGUILayout.EndVertical();
								}
								EditorGUILayout.Space();

								if (StyledButton("New Link"))
									moveInfo.frameLinks = AddElement<FrameLink>(moveInfo.frameLinks, null);
								
								EditorGUILayout.Space();
								EditorGUI.indentLevel -= 1;

							}EditorGUILayout.EndVertical();
						}
						EditorGUI.indentLevel -= 1;

					}EditorGUILayout.EndVertical();
				}
				//GUILayout.Box("", "TimeScrubber", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(1)});
				// End Move Link Options
				
				
			}EditorGUILayout.EndVertical();
			// End Move Link Options
#endif

			// Begin Particle Effects Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					particleEffectsOptions = EditorGUILayout.Foldout(particleEffectsOptions, "Particle Effects ("+ moveInfo.particleEffects.Length +")", EditorStyles.foldout);
					helpButton("move:particleeffects");
				}EditorGUILayout.EndHorizontal();

				if (particleEffectsOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUI.indentLevel += 1;
						List<int> castingValues = new List<int>();
						foreach(MoveParticleEffect particleEffect in moveInfo.particleEffects) 
							castingValues.Add(particleEffect.castingFrame);
						StyledMarker("Casting Timeline", castingValues.ToArray(), moveInfo.totalFrames, EditorGUI.indentLevel);
							
						for (int i = 0; i < moveInfo.particleEffects.Length; i ++){
							EditorGUILayout.Space();
							EditorGUILayout.BeginVertical(arrayElementStyle);{
								EditorGUILayout.Space();
								EditorGUILayout.BeginHorizontal();{
									moveInfo.particleEffects[i].castingFrame = EditorGUILayout.IntSlider("Casting Frame:", moveInfo.particleEffects[i].castingFrame, 0, moveInfo.totalFrames);
									if (GUILayout.Button("", "PaneOptions")){
										PaneOptions<MoveParticleEffect>(moveInfo.particleEffects, moveInfo.particleEffects[i], delegate (MoveParticleEffect[] newElement) { moveInfo.particleEffects = newElement; });
									}
								}EditorGUILayout.EndHorizontal();
								if (moveInfo.particleEffects[i].particleEffect == null) moveInfo.particleEffects[i].particleEffect = new ParticleInfo();
								moveInfo.particleEffects[i].particleEffect.prefab = (GameObject) EditorGUILayout.ObjectField("Particle Effect:", moveInfo.particleEffects[i].particleEffect.prefab, typeof(UnityEngine.GameObject), true);
								moveInfo.particleEffects[i].particleEffect.duration = EditorGUILayout.FloatField("Duration (seconds):", moveInfo.particleEffects[i].particleEffect.duration);
								moveInfo.particleEffects[i].particleEffect.stick = EditorGUILayout.Toggle("Sticky", moveInfo.particleEffects[i].particleEffect.stick, toggleStyle);
								moveInfo.particleEffects[i].particleEffect.bodyPart = (BodyPart) EditorGUILayout.EnumPopup("Body Part:", moveInfo.particleEffects[i].particleEffect.bodyPart, enumStyle);
								//moveInfo.particleEffects[i].particleEffect.offSet = EditorGUILayout.Vector3Field("Off Set (relative):", moveInfo.particleEffects[i].particleEffect.offSet);
								EditorGUILayout.Space();
							}EditorGUILayout.EndVertical();
						}
						if (StyledButton("New Particle Effect"))
							moveInfo.particleEffects = AddElement<MoveParticleEffect>(moveInfo.particleEffects, new MoveParticleEffect());
							
						EditorGUI.indentLevel -= 1;

					}EditorGUILayout.EndVertical();
				}
				
			}EditorGUILayout.EndVertical();
            // End Particle Effects Options

			// Begin CallNpc Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					callNpcOptions = EditorGUILayout.Foldout(callNpcOptions, "Call Npc Options (" + moveInfo.callNpcs.Length + ")", EditorStyles.foldout);
				}EditorGUILayout.EndHorizontal();

				if (callNpcOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUI.indentLevel += 1;
						List<int> castingValues = new List<int>();
						foreach(CallNpc callNpc in moveInfo.callNpcs) 
							castingValues.Add(callNpc.castingFrame);
						StyledMarker("Casting Timeline", castingValues.ToArray(), moveInfo.totalFrames, EditorGUI.indentLevel);

						for (int i = 0; i < moveInfo.callNpcs.Length; i ++){
							EditorGUILayout.Space();
							EditorGUILayout.BeginVertical(arrayElementStyle);{
								EditorGUILayout.Space();
								EditorGUILayout.BeginHorizontal();{
									moveInfo.callNpcs[i].castingFrame = EditorGUILayout.IntSlider("Casting Frame:", moveInfo.callNpcs[i].castingFrame, 0, moveInfo.totalFrames);
									if (GUILayout.Button("", "PaneOptions")){
										PaneOptions<CallNpc>(moveInfo.callNpcs, moveInfo.callNpcs[i], delegate (CallNpc[] newElement) { moveInfo.callNpcs = newElement; });
									}
								}EditorGUILayout.EndHorizontal();
								moveInfo.callNpcs[i].characterInfo = (CharacterInfo)EditorGUILayout.ObjectField("CharacterInfo:", moveInfo.callNpcs[i].characterInfo, typeof(CharacterInfo), true);
								EditorGUILayout.Space();
							}EditorGUILayout.EndVertical();
						}
						if (StyledButton("New Call Npc"))
							moveInfo.callNpcs = AddElement<CallNpc>(moveInfo.callNpcs, new CallNpc());

						EditorGUI.indentLevel -= 1;

					}EditorGUILayout.EndVertical();
				}

			}EditorGUILayout.EndVertical();
			// End CallNpc Options

            // Begin Sound Options
            EditorGUILayout.BeginVertical(rootGroupStyle);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    soundOptions = EditorGUILayout.Foldout(soundOptions, "Sound Effects (" + moveInfo.soundEffects.Length + ")", EditorStyles.foldout);
                    helpButton("move:soundeffects");
                }
                EditorGUILayout.EndHorizontal();

                if (soundOptions)
                {
                    EditorGUILayout.BeginVertical(subGroupStyle);
                    {
                        EditorGUI.indentLevel += 1;
                        List<int> castingValues = new List<int>();
                        foreach (SoundEffect soundEffect in moveInfo.soundEffects)
                            castingValues.Add(soundEffect.castingFrame);
                        StyledMarker("Casting Timeline", castingValues.ToArray(), moveInfo.totalFrames, EditorGUI.indentLevel);

                        for (int i = 0; i < moveInfo.soundEffects.Length; i++)
                        {
                            EditorGUILayout.Space();
                            EditorGUILayout.BeginVertical(arrayElementStyle);
                            {
                                EditorGUILayout.Space();
                                EditorGUILayout.BeginHorizontal();
                                {
                                    moveInfo.soundEffects[i].castingFrame = EditorGUILayout.IntSlider("Casting Frame:", moveInfo.soundEffects[i].castingFrame, 0, moveInfo.totalFrames);
                                    if (GUILayout.Button("", "PaneOptions"))
                                    {
                                        PaneOptions<SoundEffect>(moveInfo.soundEffects, moveInfo.soundEffects[i], delegate (SoundEffect[] newElement) { moveInfo.soundEffects = newElement; });
                                    }
                                }
                                EditorGUILayout.EndHorizontal();

                                moveInfo.soundEffects[i].soundEffectsToggle = EditorGUILayout.Foldout(moveInfo.soundEffects[i].soundEffectsToggle, "Possible Sound Effects (" + moveInfo.soundEffects[i].sounds.Length + ")", EditorStyles.foldout);
                                if (moveInfo.soundEffects[i].soundEffectsToggle)
                                {
                                    EditorGUILayout.BeginVertical(subGroupStyle);
                                    {
                                        EditorGUIUtility.labelWidth = 150;
                                        for (int k = 0; k < moveInfo.soundEffects[i].sounds.Length; k++)
                                        {
                                            EditorGUILayout.Space();
                                            EditorGUILayout.BeginVertical(subArrayElementStyle);
                                            {
                                                EditorGUILayout.Space();
                                                EditorGUILayout.BeginHorizontal();
                                                {
                                                    moveInfo.soundEffects[i].sounds[k] = (AudioClip)EditorGUILayout.ObjectField("Audio Clip:", moveInfo.soundEffects[i].sounds[k], typeof(UnityEngine.AudioClip), true);
                                                    if (GUILayout.Button("", "PaneOptions"))
                                                    {
                                                        PaneOptions<AudioClip>(moveInfo.soundEffects[i].sounds, moveInfo.soundEffects[i].sounds[k], delegate (AudioClip[] newElement) { moveInfo.soundEffects[i].sounds = newElement; });
                                                    }
                                                }
                                                EditorGUILayout.EndHorizontal();
                                                EditorGUILayout.Space();
                                            }
                                            EditorGUILayout.EndVertical();
                                        }
                                        if (StyledButton("New Sound Effect"))
                                            moveInfo.soundEffects[i].sounds = AddElement<AudioClip>(moveInfo.soundEffects[i].sounds, new AudioClip());

                                    }
                                    EditorGUILayout.EndVertical();
                                }

                                EditorGUILayout.Space();
                            }
                            EditorGUILayout.EndVertical();
                        }
                        if (StyledButton("New Sound Effect"))
                            moveInfo.soundEffects = AddElement<SoundEffect>(moveInfo.soundEffects, new SoundEffect());

                        EditorGUI.indentLevel -= 1;

                    }
                    EditorGUILayout.EndVertical();
                }

            }
            EditorGUILayout.EndVertical();
            // End Sound Options

            // Begin Self Applied Force Options
            EditorGUILayout.BeginVertical(rootGroupStyle);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    selfAppliedForceOptions = EditorGUILayout.Foldout(selfAppliedForceOptions, "Self Applied Forces (" + moveInfo.appliedForces.Length + ")", EditorStyles.foldout);
                    helpButton("move:selfappliedforce");
                }
                EditorGUILayout.EndHorizontal();

                if (selfAppliedForceOptions)
                {
                    EditorGUILayout.BeginVertical(subGroupStyle);
                    {
                        EditorGUI.indentLevel += 1;
                        List<int> castingValues = new List<int>();
                        foreach (AppliedForce appliedForce in moveInfo.appliedForces)
                            castingValues.Add(appliedForce.castingFrame);
                        StyledMarker("Casting Timeline", castingValues.ToArray(), moveInfo.totalFrames, EditorGUI.indentLevel);

                        for (int i = 0; i < moveInfo.appliedForces.Length; i++)
                        {
                            EditorGUILayout.Space();
                            EditorGUILayout.BeginVertical(arrayElementStyle);
                            {
                                EditorGUILayout.Space();
                                EditorGUILayout.BeginHorizontal();
                                {
                                    moveInfo.appliedForces[i].castingFrame = EditorGUILayout.IntSlider("Casting Frame:", moveInfo.appliedForces[i].castingFrame, 0, moveInfo.totalFrames);
                                    if (GUILayout.Button("", "PaneOptions"))
                                    {
                                        PaneOptions<AppliedForce>(moveInfo.appliedForces, moveInfo.appliedForces[i], delegate (AppliedForce[] newElement) { moveInfo.appliedForces = newElement; });
                                    }
                                }
                                EditorGUILayout.EndHorizontal();
                                //moveInfo.appliedForces[i].resetPreviousHorizontal = EditorGUILayout.Toggle("Reset X Force", moveInfo.appliedForces[i].resetPreviousHorizontal, toggleStyle);
                                //moveInfo.appliedForces[i].resetPreviousVertical = EditorGUILayout.Toggle("Reset Y Force", moveInfo.appliedForces[i].resetPreviousVertical, toggleStyle);
                                moveInfo.appliedForces[i].force = EditorGUILayout.Vector2Field("Force Applied:", moveInfo.appliedForces[i].force);
                                moveInfo.appliedForces[i].forceTweenTime = EditorGUILayout.FloatField("ForceTweenTime:", moveInfo.appliedForces[i].forceTweenTime);
                                EditorGUILayout.Space();
                            }
                            EditorGUILayout.EndVertical();
                        }
                        if (StyledButton("New Applied Force"))
                            moveInfo.appliedForces = AddElement<AppliedForce>(moveInfo.appliedForces, new AppliedForce());

                        EditorGUI.indentLevel -= 1;

                    }
                    EditorGUILayout.EndVertical();
                }

            }
            EditorGUILayout.EndVertical();
            // End Self Applied Force Options

            // Begin Body Parts Visibility Options
            EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					bodyPartVisibilityOptions = EditorGUILayout.Foldout(bodyPartVisibilityOptions, "Body Parts Visibility Changes (" + moveInfo.bodyPartVisibilityChanges.Length + ")", EditorStyles.foldout);
					helpButton("move:bodyPartVisibilityChanges");
				}EditorGUILayout.EndHorizontal();
				
				if (bodyPartVisibilityOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUI.indentLevel += 1;
						List<int> castingValues = new List<int>();
						foreach(BodyPartVisibilityChange bodyPartVisibilityChangesEffect in moveInfo.bodyPartVisibilityChanges) 
							castingValues.Add(bodyPartVisibilityChangesEffect.castingFrame);
						StyledMarker("Casting Timeline", castingValues.ToArray(), moveInfo.totalFrames, EditorGUI.indentLevel);
						
						for (int i = 0; i < moveInfo.bodyPartVisibilityChanges.Length; i ++){
							EditorGUILayout.Space();
							EditorGUILayout.BeginVertical(arrayElementStyle);{
								EditorGUILayout.Space();
								EditorGUILayout.BeginHorizontal();{
									moveInfo.bodyPartVisibilityChanges[i].castingFrame = EditorGUILayout.IntSlider("Casting Frame:", moveInfo.bodyPartVisibilityChanges[i].castingFrame, 0, moveInfo.totalFrames);
									if (GUILayout.Button("", "PaneOptions")){
										PaneOptions<BodyPartVisibilityChange>(moveInfo.bodyPartVisibilityChanges, moveInfo.bodyPartVisibilityChanges[i], delegate (BodyPartVisibilityChange[] newElement) { moveInfo.bodyPartVisibilityChanges = newElement; });
									}
								}EditorGUILayout.EndHorizontal();
								moveInfo.bodyPartVisibilityChanges[i].bodyPart = (BodyPart)EditorGUILayout.EnumPopup("Body Part:", moveInfo.bodyPartVisibilityChanges[i].bodyPart);
								moveInfo.bodyPartVisibilityChanges[i].visible = EditorGUILayout.Toggle("Visible:", moveInfo.bodyPartVisibilityChanges[i].visible);
								EditorGUILayout.Space();
							}EditorGUILayout.EndVertical();
						}
						if (StyledButton("New Visibility Change"))
							moveInfo.bodyPartVisibilityChanges = AddElement<BodyPartVisibilityChange>(moveInfo.bodyPartVisibilityChanges, new BodyPartVisibilityChange());
						
						EditorGUI.indentLevel -= 1;
						
					}EditorGUILayout.EndVertical();
				}
				
			}EditorGUILayout.EndVertical();
			// End Body Parts Visibility Options

			// Begin Projectile Options
			EditorGUILayout.BeginVertical(rootGroupStyle);{
				EditorGUILayout.BeginHorizontal();{
					projectileOptions = EditorGUILayout.Foldout(projectileOptions, "Projectiles ("+ moveInfo.projectiles.Length +")", EditorStyles.foldout);
					helpButton("move:projectiles");
				}EditorGUILayout.EndHorizontal();

				if (projectileOptions){
					EditorGUILayout.BeginVertical(subGroupStyle);{
						EditorGUI.indentLevel += 1;
						List<int> castingValues = new List<int>();
						foreach(Projectile projectile in moveInfo.projectiles) 
							castingValues.Add(projectile.castingFrame);
						StyledMarker("Casting Timeline", castingValues.ToArray(), moveInfo.totalFrames, EditorGUI.indentLevel);
							
						for (int i = 0; i < moveInfo.projectiles.Length; i ++){
							EditorGUILayout.Space();
							EditorGUILayout.BeginVertical(arrayElementStyle);{
								EditorGUILayout.Space();
								EditorGUILayout.BeginHorizontal();{
									moveInfo.projectiles[i].castingFrame = EditorGUILayout.IntSlider("Casting Frame:", moveInfo.projectiles[i].castingFrame, 0, moveInfo.totalFrames);
									if (GUILayout.Button("", "PaneOptions")){
										PaneOptions<Projectile>(moveInfo.projectiles, moveInfo.projectiles[i], delegate (Projectile[] newElement) { moveInfo.projectiles = newElement; });
									}
								}EditorGUILayout.EndHorizontal();
								EditorGUILayout.Space();
								SubGroupTitle("Prefabs");
								EditorGUILayout.Space();
								moveInfo.projectiles[i].projectilePrefab = (GameObject) EditorGUILayout.ObjectField("Projectile Prefab:", moveInfo.projectiles[i].projectilePrefab, typeof(UnityEngine.GameObject), true);
                                moveInfo.projectiles[i].selfRotationSpeed = EditorGUILayout.Slider("SelfRotationSpeed:", moveInfo.projectiles[i].selfRotationSpeed, 0, 360);
                                moveInfo.projectiles[i].impactPrefab = (GameObject) EditorGUILayout.ObjectField("Impact Prefab:", moveInfo.projectiles[i].impactPrefab, typeof(UnityEngine.GameObject), true);

                                if (moveInfo.projectiles[i].impactPrefab != null)
                                {
									EditorGUIUtility.labelWidth = 190;
									moveInfo.projectiles[i].impactDuration = EditorGUILayout.FloatField("Impact Duration (seconds):", moveInfo.projectiles[i].impactDuration);
									EditorGUIUtility.labelWidth = 150;
								}

								if (projectilePrefab == null){
									if (StyledButton("Preview")){
										if (moveInfo.projectiles[i].projectilePrefab == null) {
											characterWarning = true;
											errorMsg = "'Projectile Prefab' not found.";
										}else if (EditorApplication.isPlayingOrWillChangePlaymode){
											characterWarning = true;
											errorMsg = "You can't preview while in play mode.";
										}else{
											characterWarning = false;
											EditorCamera.SetPosition(Vector3.up * 4);
											EditorCamera.SetRotation(Quaternion.identity);
											EditorCamera.SetOrthographic(true);
											EditorCamera.SetSize(10);
											
											foreach(Projectile projectile in moveInfo.projectiles)
												projectile.preview = false;
											
											moveInfo.projectiles[i].preview = true;
											
											projectilePrefab = (GameObject) PrefabUtility.InstantiatePrefab(moveInfo.projectiles[i].projectilePrefab);
											ProjectileSampler(projectilePrefab, moveInfo.projectiles[i]);
										}
									}
								}else{
									EditorGUILayout.Space();
									EditorGUILayout.Space();
									EditorGUILayout.BeginHorizontal();{
										if (StyledButton("Reset Scene View")){
											EditorCamera.SetPosition(Vector3.up * 4);
											EditorCamera.SetRotation(Quaternion.identity);
											EditorCamera.SetOrthographic(true);
											EditorCamera.SetSize(10);
										}
										if (StyledButton("Close Preview")) {
											Editor.DestroyImmediate(projectilePrefab);
											projectilePrefab = null;
										}
									}EditorGUILayout.EndHorizontal();
									EditorGUILayout.Space();
								}

								EditorGUI.BeginDisabledGroup(moveInfo.projectiles[i].projectilePrefab == null);{
									EditorGUILayout.Space();
									SubGroupTitle("Casting Options");
									EditorGUILayout.Space();
									EditorGUIUtility.labelWidth = 180;
									moveInfo.projectiles[i].bodyPart = (BodyPart) EditorGUILayout.EnumPopup("Body Part Origin:", moveInfo.projectiles[i].bodyPart, enumStyle);
                                    moveInfo.projectiles[i].moveableType = (ProjectileMoveableType) EditorGUILayout.EnumPopup("MoveableType:", moveInfo.projectiles[i].moveableType, enumStyle);
                                    if(moveInfo.projectiles[i].moveableType == ProjectileMoveableType.normal)
                                    {
                                        moveInfo.projectiles[i].movementType = (ProjectileMovementUtil.MovementType)EditorGUILayout.EnumPopup("Movement Type:", moveInfo.projectiles[i].movementType, enumStyle);
                                        moveInfo.projectiles[i].speed = EditorGUILayout.IntSlider("Speed:", moveInfo.projectiles[i].speed, 0, 50);
                                        moveInfo.projectiles[i].duration = EditorGUILayout.FloatField("Duration (Seconds):", moveInfo.projectiles[i].duration);
                                        moveInfo.projectiles[i].directionAngle = EditorGUILayout.FloatField("DirectionAngle:", moveInfo.projectiles[i].directionAngle);
                           
                                        if (moveInfo.projectiles[i].movementType == ProjectileMovementUtil.MovementType.parabola)
                                        {
                                            moveInfo.projectiles[i].gravity = EditorGUILayout.FloatField("Gravity:", moveInfo.projectiles[i].gravity);
                                        }
                                    }
                                    else if (moveInfo.projectiles[i].moveableType == ProjectileMoveableType.laser)
                                    {
                                        moveInfo.projectiles[i].range = EditorGUILayout.Slider("Range:", moveInfo.projectiles[i].range, 0f, 50f);
                                        moveInfo.projectiles[i].duration = EditorGUILayout.FloatField("Duration (Seconds):", moveInfo.projectiles[i].duration);
                                    }
                                    else if (moveInfo.projectiles[i].moveableType == ProjectileMoveableType.boxWithoutMove)
                                    {
                                        moveInfo.projectiles[i].rangeOfV2 = EditorGUILayout.Vector2Field("Range:", moveInfo.projectiles[i].rangeOfV2);
                                    }
                                    moveInfo.projectiles[i].throughWallType = (ProjectileMovementUtil.ThroughType)EditorGUILayout.EnumPopup("Through Wall Type:", moveInfo.projectiles[i].throughWallType, enumStyle);
                                    moveInfo.projectiles[i].throughEnemyType = (ProjectileMovementUtil.ThroughType)EditorGUILayout.EnumPopup("Through Enemy Type:", moveInfo.projectiles[i].throughEnemyType, enumStyle);
                                    EditorGUILayout.Space();
									SubGroupTitle("Collision Options");
									EditorGUILayout.Space();
                                    EditorGUIUtility.labelWidth = 200;

									moveInfo.projectiles[i].hitStrength = (HitStrengh)EditorGUILayout.EnumPopup("Hit Strength:", moveInfo.projectiles[i].hitStrength, enumStyle);
                                    moveInfo.projectiles[i].pushForce = EditorGUILayout.Vector2Field("Push Force:", moveInfo.projectiles[i].pushForce);
                                    moveInfo.projectiles[i].pushForceTime = EditorGUILayout.FloatField("Push ForceTime:", moveInfo.projectiles[i].pushForceTime);
                                    moveInfo.projectiles[i].debuffType = (DebuffType)EditorGUILayout.EnumPopup("Debuff Type:", moveInfo.projectiles[i].debuffType, enumStyle);
                                    moveInfo.projectiles[i].debuffParam = EditorGUILayout.FloatField("Debuff Param:", moveInfo.projectiles[i].debuffParam);

                                    moveInfo.projectiles[i].spaceBetweenHits = (Sizes)EditorGUILayout.EnumPopup("Space Between Hits:", moveInfo.projectiles[i].spaceBetweenHits, enumStyle);

                                    moveInfo.projectiles[i].overrideHitEffects = EditorGUILayout.Toggle("Override Hit Effects", moveInfo.projectiles[i].overrideHitEffects, toggleStyle);
                                    if (moveInfo.projectiles[i].overrideHitEffects)
                                    {
                                        HitOptionBlock("Hit Effects", moveInfo.projectiles[i].hitEffects);
                                    }
                                    moveInfo.projectiles[i].overrideHitAnimation = EditorGUILayout.Toggle("Override Hit Animation", moveInfo.projectiles[i].overrideHitAnimation, toggleStyle);
                                    if (moveInfo.projectiles[i].overrideHitAnimation)
                                    {
                                        moveInfo.projectiles[i].newHitAnimation = (BasicMoveReference)EditorGUILayout.EnumPopup("- Hit Animation:", moveInfo.projectiles[i].newHitAnimation, enumStyle);
                                    }

                                    EditorGUILayout.Space();		
								}EditorGUI.EndDisabledGroup();

							}EditorGUILayout.EndVertical();
							EditorGUILayout.Space();
						}
						if (StyledButton("New Projectile"))
							moveInfo.projectiles = AddElement<Projectile>(moveInfo.projectiles, new Projectile());
							
						EditorGUI.indentLevel -= 1;
					
					}EditorGUILayout.EndVertical();
				}
			}EditorGUILayout.EndVertical();
			// End Projectile Options

		}EditorGUILayout.EndScrollView();

		if (GUI.changed) {
			Undo.RecordObject(moveInfo, "Move Editor Modify");
			EditorUtility.SetDirty(moveInfo);
		}
	}
	
	private void PlayerConditionsGroup(string _name, PlayerConditions playerConditions, bool extraIndent){
		if (extraIndent) EditorGUILayout.BeginVertical(subGroupStyle);
			//SubGroupTitle(_name);
			if (extraIndent) EditorGUI.indentLevel += 1;
			/*playerConditions.basicMovesToggle = EditorGUILayout.Foldout(playerConditions.basicMovesToggle, "Basic Moves Filter ("+ playerConditions.basicMoveLimitation.Length +")", EditorStyles.foldout);
			if (playerConditions.basicMovesToggle){
				EditorGUILayout.BeginVertical(subGroupStyle);{
					EditorGUILayout.Space();
					EditorGUI.indentLevel += 1;
					if (playerConditions.basicMoveLimitation != null){
						for (int y = 0; y < playerConditions.basicMoveLimitation.Length; y ++){
							EditorGUILayout.Space();
							EditorGUILayout.BeginVertical(subArrayElementStyle);{
								EditorGUILayout.BeginHorizontal();{
									playerConditions.basicMoveLimitation[y] = (BasicMoveReference)EditorGUILayout.EnumPopup("Basic Move:", playerConditions.basicMoveLimitation[y], enumStyle);
									if (GUILayout.Button("", "PaneOptions")){
										PaneOptions<BasicMoveReference>(playerConditions.basicMoveLimitation, playerConditions.basicMoveLimitation[y], delegate (BasicMoveReference[] newElement) { playerConditions.basicMoveLimitation = newElement; });
									}
								}EditorGUILayout.EndHorizontal();
								EditorGUILayout.Space();
							}EditorGUILayout.EndVertical();
						}
					}
					if (StyledButton("New Basic Move"))
						playerConditions.basicMoveLimitation = AddElement<BasicMoveReference>(playerConditions.basicMoveLimitation, BasicMoveReference.Idle);

				}EditorGUILayout.EndVertical();

				EditorGUI.indentLevel -= 1;
			}*/
			
			
			playerConditions.statesToggle = EditorGUILayout.Foldout(playerConditions.statesToggle, "Possible States ("+ playerConditions.possibleMoveStates.Length +")", EditorStyles.foldout);
			if (playerConditions.statesToggle){
				EditorGUILayout.BeginVertical(subGroupStyle);{
					EditorGUI.indentLevel += 1;
					for (int i = 0; i < playerConditions.possibleMoveStates.Length; i++){
						EditorGUILayout.Space();
						EditorGUILayout.BeginVertical(subArrayElementStyle);{
							EditorGUILayout.Space();
							EditorGUILayout.BeginHorizontal();{
                            //playerConditions.possibleMoveStates[i].possibleState = (PossibleStates)EditorGUILayout.EnumPopup("State:", playerConditions.possibleMoveStates[i].possibleState, enumStyle);
                            playerConditions.possibleMoveStates[i].possibleState = PossibleStates.Stand;
                            if (GUILayout.Button("", "PaneOptions")){
									PaneOptions<PossibleMoveStates>(playerConditions.possibleMoveStates, playerConditions.possibleMoveStates[i], delegate (PossibleMoveStates[] newElement) { playerConditions.possibleMoveStates = newElement; });
								}
							}EditorGUILayout.EndHorizontal();

                        playerConditions.possibleMoveStates[i].opponentDistance = (CharacterDistance)EditorGUILayout.EnumPopup("Opponent Distance:", playerConditions.possibleMoveStates[i].opponentDistance, enumStyle);
                        if (playerConditions.possibleMoveStates[i].opponentDistance == CharacterDistance.Any) {
                            playerConditions.possibleMoveStates[i].proximityRangeBegins = 0;
                            playerConditions.possibleMoveStates[i].proximityRangeEnds = 10;
                        }else if (playerConditions.possibleMoveStates[i].opponentDistance == CharacterDistance.VeryClose) {
                            playerConditions.possibleMoveStates[i].proximityRangeBegins = 0;
                            playerConditions.possibleMoveStates[i].proximityRangeEnds = 1;
                        }else if (playerConditions.possibleMoveStates[i].opponentDistance == CharacterDistance.Close) {
                            playerConditions.possibleMoveStates[i].proximityRangeBegins = 0;
                            playerConditions.possibleMoveStates[i].proximityRangeEnds = 3;
                        }else if (playerConditions.possibleMoveStates[i].opponentDistance == CharacterDistance.Mid) {
                            playerConditions.possibleMoveStates[i].proximityRangeBegins = 3;
                            playerConditions.possibleMoveStates[i].proximityRangeEnds = 5;
                        }else if (playerConditions.possibleMoveStates[i].opponentDistance == CharacterDistance.Far) {
                            playerConditions.possibleMoveStates[i].proximityRangeBegins = 5;
                            playerConditions.possibleMoveStates[i].proximityRangeEnds = 10;
                        }else if (playerConditions.possibleMoveStates[i].opponentDistance == CharacterDistance.VeryFar) {
                            playerConditions.possibleMoveStates[i].proximityRangeBegins = 7;
                            playerConditions.possibleMoveStates[i].proximityRangeEnds = 10;
                        }

                        int pArcBeginsTemp = playerConditions.possibleMoveStates[i].proximityRangeBegins;
                        int pArcEndsTemp = playerConditions.possibleMoveStates[i].proximityRangeEnds;
                        EditorGUI.indentLevel += 2;
                        StyledMinMaxSlider("Proximity", ref playerConditions.possibleMoveStates[i].proximityRangeBegins, ref playerConditions.possibleMoveStates[i].proximityRangeEnds, 0, 10, EditorGUI.indentLevel);
                        EditorGUI.indentLevel -= 2;
                        if (playerConditions.possibleMoveStates[i].proximityRangeBegins != pArcBeginsTemp ||
                            playerConditions.possibleMoveStates[i].proximityRangeEnds != pArcEndsTemp){
                            playerConditions.possibleMoveStates[i].opponentDistance = CharacterDistance.Other;
                        }

                        EditorGUILayout.Space();
							
							if (playerConditions.possibleMoveStates[i].possibleState == PossibleStates.StraightJump ||
							    playerConditions.possibleMoveStates[i].possibleState == PossibleStates.ForwardJump ||
							    playerConditions.possibleMoveStates[i].possibleState == PossibleStates.BackJump){
								/*playerConditions.possibleMoveStates[i].jumpArc = (JumpArc)EditorGUILayout.EnumPopup("Jump Arc:", playerConditions.possibleMoveStates[i].jumpArc, enumStyle);
								if (playerConditions.possibleMoveStates[i].jumpArc == JumpArc.Any) {
									playerConditions.possibleMoveStates[i].jumpArcBegins = 0;
									playerConditions.possibleMoveStates[i].jumpArcEnds = 100;
								}else if (playerConditions.possibleMoveStates[i].jumpArc == JumpArc.TakeOff) {
									playerConditions.possibleMoveStates[i].jumpArcBegins = 0;
									playerConditions.possibleMoveStates[i].jumpArcEnds = 35;
								}else if (playerConditions.possibleMoveStates[i].jumpArc == JumpArc.Jumping) {
									playerConditions.possibleMoveStates[i].jumpArcBegins = 0;
									playerConditions.possibleMoveStates[i].jumpArcEnds = 60;
								}else if (playerConditions.possibleMoveStates[i].jumpArc == JumpArc.Top) {
									playerConditions.possibleMoveStates[i].jumpArcBegins = 30;
									playerConditions.possibleMoveStates[i].jumpArcEnds = 70;
								}else if (playerConditions.possibleMoveStates[i].jumpArc == JumpArc.Falling) {
									playerConditions.possibleMoveStates[i].jumpArcBegins = 50;
									playerConditions.possibleMoveStates[i].jumpArcEnds = 100;
								}else if (playerConditions.possibleMoveStates[i].jumpArc == JumpArc.Landing) {
									playerConditions.possibleMoveStates[i].jumpArcBegins = 65;
									playerConditions.possibleMoveStates[i].jumpArcEnds = 100;
								}
								
								int arcBeginsTemp = playerConditions.possibleMoveStates[i].jumpArcBegins;
								int arcEndsTemp = playerConditions.possibleMoveStates[i].jumpArcEnds;
								EditorGUI.indentLevel += 2;
								StyledMinMaxSlider("Jump Arc (%)", ref playerConditions.possibleMoveStates[i].jumpArcBegins, ref playerConditions.possibleMoveStates[i].jumpArcEnds, 0, 100, EditorGUI.indentLevel);
								EditorGUI.indentLevel -= 2;
								if (playerConditions.possibleMoveStates[i].jumpArcBegins != arcBeginsTemp ||
								    playerConditions.possibleMoveStates[i].jumpArcEnds != arcEndsTemp){
									playerConditions.possibleMoveStates[i].jumpArc = JumpArc.Other;
								}*/
								
							}else if (playerConditions.possibleMoveStates[i].possibleState == PossibleStates.Stand){
								//playerConditions.possibleMoveStates[i].standBy = EditorGUILayout.Toggle("Idle", playerConditions.possibleMoveStates[i].standBy, toggleStyle);
								//playerConditions.possibleMoveStates[i].movingForward = EditorGUILayout.Toggle("Moving Forward", playerConditions.possibleMoveStates[i].movingForward, toggleStyle);
								//playerConditions.possibleMoveStates[i].movingBack = EditorGUILayout.Toggle("Moving Back", playerConditions.possibleMoveStates[i].movingBack, toggleStyle);
                                //playerConditions.possibleMoveStates[i].isInAir = EditorGUILayout.Toggle("InAir", playerConditions.possibleMoveStates[i].isInAir, toggleStyle);
                                //playerConditions.possibleMoveStates[i].isNotInAir = EditorGUILayout.Toggle("NotInAir", playerConditions.possibleMoveStates[i].isNotInAir, toggleStyle);
                                //playerConditions.possibleMoveStates[i].isNeedBoom = EditorGUILayout.Toggle("NeedBoom", playerConditions.possibleMoveStates[i].isNeedBoom, toggleStyle);
                                //playerConditions.possibleMoveStates[i].isNeedKnife = EditorGUILayout.Toggle("NeedKnife", playerConditions.possibleMoveStates[i].isNeedKnife, toggleStyle);
                                playerConditions.possibleMoveStates[i].isContinueNormalAttack = EditorGUILayout.Toggle("ContinueAttack", playerConditions.possibleMoveStates[i].isContinueNormalAttack, toggleStyle);
                                //playerConditions.possibleMoveStates[i].isFast = EditorGUILayout.Toggle("FastFly", playerConditions.possibleMoveStates[i].isFast, toggleStyle);
                                //playerConditions.possibleMoveStates[i].isNotFast = EditorGUILayout.Toggle("NotFastFly", playerConditions.possibleMoveStates[i].isNotFast, toggleStyle);
								playerConditions.possibleMoveStates[i].weaponType = (WeaponType)EditorGUILayout.EnumPopup("WeaponType:", playerConditions.possibleMoveStates[i].weaponType, enumStyle);
								if (playerConditions.possibleMoveStates[i].weaponType != WeaponType.None) {
									playerConditions.possibleMoveStates[i].weaponIndex = EditorGUILayout.IntSlider("WeaponId:", playerConditions.possibleMoveStates[i].weaponIndex, -100, 100);
								}
                            }
						
							/*if (playerConditions.possibleMoveStates[i].possibleState != PossibleStates.Down){
								playerConditions.possibleMoveStates[i].blocking = EditorGUILayout.Toggle("Blocking", playerConditions.possibleMoveStates[i].blocking, toggleStyle);
								playerConditions.possibleMoveStates[i].stunned = EditorGUILayout.Toggle("Stunned", playerConditions.possibleMoveStates[i].stunned, toggleStyle);
							}*/
							
							EditorGUILayout.Space();
						}EditorGUILayout.EndVertical();
					}
					if (StyledButton("New Possible Move State"))
						playerConditions.possibleMoveStates = AddElement<PossibleMoveStates>(playerConditions.possibleMoveStates, null);
					
					EditorGUI.indentLevel -= 1;
				}EditorGUILayout.EndVertical();

			}
            if (extraIndent) EditorGUI.indentLevel -= 1;
			
		if (extraIndent) EditorGUILayout.EndVertical();
	}

	private void HitOptionBlock(string label, HitTypeOptions hit){
		hit.editorToggle = EditorGUILayout.Foldout(hit.editorToggle, label, foldStyle);
		if (hit.editorToggle){
			EditorGUILayout.BeginVertical(subGroupStyle);{
				EditorGUILayout.Space();
				EditorGUI.indentLevel += 1;
				
				hit.hitParticle = (GameObject) EditorGUILayout.ObjectField("Particle Effect:", hit.hitParticle, typeof(UnityEngine.GameObject), true);
				hit.killTime = EditorGUILayout.FloatField("Effect Duration:", hit.killTime);

				hit.freezingTime = EditorGUILayout.FloatField("Freezing Time:", hit.freezingTime);
				hit.animationSpeed = EditorGUILayout.FloatField("Animation Speed (%):", hit.animationSpeed);
				hit.shakeCharacterOnHit = EditorGUILayout.Toggle("Shake Character On Hit", hit.shakeCharacterOnHit);
				hit.shakeCameraOnHit = EditorGUILayout.Toggle("Shake Camera On Hit", hit.shakeCameraOnHit);
				hit.shakeDensity = EditorGUILayout.FloatField("Shake Density:", hit.shakeDensity);
                hit.moveSpeed = EditorGUILayout.FloatField("Move Speed (%):", hit.moveSpeed);
                hit.moveSpeedTime = EditorGUILayout.FloatField("MoveSpeed Time:", hit.moveSpeedTime);

                EditorGUI.indentLevel -= 1;
				EditorGUILayout.Space();
				
			}EditorGUILayout.EndVertical();
		}
	}

	private void SubGroupTitle(string _name){
		GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label(_name);
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
	}

	public int StyledSlider (string label, int targetVar, int indentLevel, int minValue, int maxValue)
    {
		int indentSpacing = 25 * indentLevel;			
		EditorGUILayout.Space();
		EditorGUILayout.Space();

		Rect tempRect = GUILayoutUtility.GetRect(1, 10);

		Rect rect = new Rect(indentSpacing,tempRect.y, Screen.width - indentSpacing - 100, 20);
		EditorGUI.ProgressBar(rect, Mathf.Abs((float)targetVar/maxValue), label);
		
		tempRect = GUILayoutUtility.GetRect(1, 20);
		rect.y += 10;
		rect.x = indentLevel * 10;
		rect.width = (Screen.width - (indentLevel * 10) - 100) + 55; // Changed for 4.3;

		return EditorGUI.IntSlider(rect, "", targetVar, minValue, maxValue);
	}
	
	public float StyledSlider (string label, float targetVar, int indentLevel, float minValue, float maxValue) {
		int indentSpacing = 25 * indentLevel;
		EditorGUILayout.Space();
		EditorGUILayout.Space();

		Rect tempRect = GUILayoutUtility.GetRect(1, 10);

		Rect rect = new Rect(indentSpacing,tempRect.y, Screen.width - indentSpacing - 100, 20);
		EditorGUI.ProgressBar(rect, Mathf.Abs((float)targetVar/maxValue), label);
		
		tempRect = GUILayoutUtility.GetRect(1, 20);
		rect.y += 10;
		rect.x = indentLevel * 10;
		rect.width = (Screen.width - (indentLevel * 10) - 100) + 55; // Changed for 4.3;

		return EditorGUI.Slider(rect, "", targetVar, minValue, maxValue);
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
	
	public void StyledMinMaxSlider (string label, ref int minValue, ref int maxValue, int minLimit, int maxLimit, int indentLevel) {
		int indentSpacing = 25 * indentLevel;
		//indentSpacing += 30;
		EditorGUILayout.Space();
		EditorGUILayout.Space();

        minValue = Mathf.Max(minValue, minLimit);
        minValue = Mathf.Min(minValue, maxValue - 1);
        maxValue = Mathf.Max(maxValue, minLimit + 1);
        maxValue = Mathf.Min(maxValue, maxLimit);

        /*if (minValue < minLimit) minValue = minLimit;
		if (maxValue < 1) maxValue = 1;
		if (maxValue > maxLimit) maxValue = maxLimit;
		if (minValue == maxValue) minValue --;*/

		float minValueFloat = (float) minValue;
		float maxValueFloat = (float) maxValue;
		float minLimitFloat = (float) minLimit;
		float maxLimitFloat = (float) maxLimit;
		
		Rect tempRect = GUILayoutUtility.GetRect(1, 10);

		Rect rect = new Rect(indentSpacing,tempRect.y, Screen.width - indentSpacing - 100, 20);
		//Rect rect = new Rect(indentSpacing + 15,tempRect.y, Screen.width - indentSpacing - 70, 20);
		float fillLeftPos = ((rect.width/maxLimitFloat) * minValueFloat) + rect.x;
		float fillRightPos = ((rect.width/maxLimitFloat) * maxValueFloat) + rect.x;
		float fillWidth = fillRightPos - fillLeftPos;
		
		//fillWidth += (rect.width/maxLimitFloat);
		//fillLeftPos -= (rect.width/maxLimitFloat);
		
		// Border
		GUI.Box(rect, "", borderBarStyle);
		
		// Overlay
		GUI.Box(new Rect(fillLeftPos, rect.y, fillWidth, rect.height), new GUIContent(), fillBarStyle1);
		
		// Text
		//GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
    	//centeredStyle.alignment = TextAnchor.UpperCenter;
		labelStyle.alignment = TextAnchor.UpperCenter;
		GUI.Label(rect, label + " between "+ Mathf.Floor(minValueFloat)+" and "+Mathf.Floor(maxValueFloat), labelStyle);
		labelStyle.alignment = TextAnchor.MiddleCenter;
		
		// Slider
		rect.y += 10;
		rect.x = indentLevel * 10;
		rect.width = (Screen.width - (indentLevel * 10) - 100);

		EditorGUI.MinMaxSlider(rect, ref minValueFloat, ref maxValueFloat, minLimitFloat, maxLimitFloat);
		minValue = (int) minValueFloat;
		maxValue = (int) maxValueFloat;

		tempRect = GUILayoutUtility.GetRect(1, 20);
	}
	
	public void StyledMarker (string label, int[] locations, int maxValue, int indentLevel) {
		if (indentLevel == 1) indentLevel++;
		int indentSpacing = 25 * indentLevel;
		
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		Rect tempRect = GUILayoutUtility.GetRect(1, 20);
		Rect rect = new Rect(indentSpacing,tempRect.y, Screen.width - indentSpacing - 60, 20);
		
		// Border
		GUI.Box(rect, "", borderBarStyle);
		
		// Overlay
		foreach(int i in locations){
			float xPos = ((rect.width/(float)maxValue) * (float)i) + rect.x;
			if (xPos + 5 > rect.width + rect.x) xPos -= 5;
			GUI.Box(new Rect(xPos, rect.y, 5, rect.height), new GUIContent(), fillBarStyle2);
		}
		
		// Text
		GUI.Label(rect, new GUIContent(label), labelStyle);
		
		tempRect = GUILayoutUtility.GetRect(1, 20);
	}

	public void StyledMarker (string label, Vector3[] locations, int maxValue, int indentLevel, bool fillBounds) {
		if (indentLevel == 1) indentLevel++;
		int indentSpacing = 25 * indentLevel;
		
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		Rect tempRect = GUILayoutUtility.GetRect(1, 20);
		Rect rect = new Rect(indentSpacing,tempRect.y, Screen.width - indentSpacing - 60, 20);
		
		// Border
		GUI.Box(rect, "", borderBarStyle);

        if (fillBounds && locations.Length > 0 && locations[0].x > 0) {
			float firstLeftPos = ((rect.width/maxValue) * locations[0].x);
			//firstLeftPos -= (rect.width/maxValue);
			GUI.Box(new Rect(rect.x, rect.y, firstLeftPos, rect.height), new GUIContent(), fillBarStyle3);
		}

		// Overlay
		float fillLeftPos = 0;
		float fillRightPos = 0;
        float lastLocation = 0;
		foreach(Vector3 i in locations){
            lastLocation = i.y;
			fillLeftPos = ((rect.width/maxValue) * i.x) + rect.x;
			fillRightPos = ((rect.width/maxValue) * i.y) + rect.x;

			float fillWidth = fillRightPos - fillLeftPos;
			//fillWidth += (rect.width/maxValue);
			//fillLeftPos -= (rect.width/maxValue);

			if (i.z > 0){
				GUI.Box(new Rect(fillLeftPos, rect.y, fillWidth, rect.height), new GUIContent(), fillBarStyle5);
			}else{
				GUI.Box(new Rect(fillLeftPos, rect.y, fillWidth, rect.height), new GUIContent(), fillBarStyle2);
			}
		}

		if (fillBounds && locations.Length > 0 && lastLocation < maxValue){
			float fillWidth = rect.width - fillRightPos + rect.x;
			GUI.Box(new Rect(fillRightPos, rect.y, fillWidth, rect.height), new GUIContent(), fillBarStyle4);
		}

		// Text
		GUI.Label(rect, new GUIContent(label), labelStyle);

        if (fillBounds && locations.Length > 0) {
			EditorGUILayout.Space();
			GUILayout.BeginHorizontal(subArrayElementStyle);{
				labelStyle.normal.textColor = Color.yellow;
                moveInfo.startUpFrames = locations[0].x <= 0 ? 0 : moveInfo.hits[0].activeFramesBegin - 1;
				GUILayout.Label("Start Up: "+ moveInfo.startUpFrames, labelStyle);
				labelStyle.normal.textColor = Color.cyan;
				moveInfo.activeFrames = (moveInfo.hits[moveInfo.hits.Length - 1].activeFramesEnds - moveInfo.hits[0].activeFramesBegin);
				GUILayout.Label("Active: "+ moveInfo.activeFrames, labelStyle);
				labelStyle.normal.textColor = Color.red;
                moveInfo.recoveryFrames = lastLocation >= maxValue ? 0 : (moveInfo.totalFrames - moveInfo.hits[moveInfo.hits.Length - 1].activeFramesEnds + 1);
				GUILayout.Label("Recovery: "+ moveInfo.recoveryFrames, labelStyle);
			}GUILayout.EndHorizontal();
		}
		labelStyle.normal.textColor = Color.white;

		//GUI.skin.label.normal.textColor = new Color(.706f, .706f, .706f, 1);
		tempRect = GUILayoutUtility.GetRect(1, 20);
	}
	
	public void AnimationSampler(GameObject targetObj, AnimationClip animationClip, int castingFrame, float speed){
		animTime = (((float)(animFrame - castingFrame)/moveInfo.fps) * speed);
		//animTime = ((float)(animFrame + startupFrame - castingFrame)/moveInfo.fps);
		if (animTime < 0) animTime = 0;
		
		Animator animator = targetObj.GetComponent<Animator>();
		if (animator == null) animator = (Animator) targetObj.AddComponent(typeof(Animator));
		if (animator.runtimeAnimatorController == null)
            animator.runtimeAnimatorController = (RuntimeAnimatorController)Resources.Load("MC_Controller");

		if (animFrame > castingFrame) animationClip.SampleAnimation(targetObj, animTime);
	}

	public void ProjectileSampler(GameObject prefab, Projectile projectile)
    {
	}

	public void AnimationSampler(GameObject targetChar, AnimationClip animationClip, int castingFrame, bool loadHitBoxes, bool loadHurtBoxes, bool mirror, bool invertRotation){

        currentSpeed = moveInfo.animationSpeed;
        animTime = ((animFrame - castingFrame) / moveInfo.fps) * currentSpeed;

        if (!moveInfo.fixedSpeed && animationClip == moveInfo.animationClip) {
            animTime = 0;
            for (int frameCounter = 0; frameCounter < animFrame; frameCounter++) {
                foreach (AnimSpeedKeyFrame speedKeyFrame in moveInfo.animSpeedKeyFrame) {
                    if (frameCounter > speedKeyFrame.castingFrame) {
                        currentSpeed = moveInfo.animationSpeed * speedKeyFrame.speed;
                    }
                }
                animTime += ((float)1 / moveInfo.fps) * currentSpeed;
            };


            /*int previousFrame = 0;
            
            int originalCastingFrame = (int)Mathf.Floor((totalFramesOriginal * animFrame) / moveInfo.totalFrames);
            animTime = (animFrame - castingFrame / moveInfo.fps) * currentSpeed; 

            foreach (AnimSpeedKeyFrame speedKeyFrame in moveInfo.animSpeedKeyFrame) {
                if (originalCastingFrame >= speedKeyFrame.castingFrame) {
                    currentSpeed = moveInfo.animationSpeed * speedKeyFrame.speed;
                    animTime += (1 / moveInfo.fps) * currentSpeed;
                    previousFrame = (int)Mathf.Floor((originalCastingFrame * moveInfo.totalFrames) / totalFramesOriginal);


                    //relativeFrame = (animFrame / currentSpeed);
                    //animTime = ((animationClip.length * animFrame) / moveInfo.totalFrames) / currentSpeed;
                }
            }*/
        }

        if (currentSpeed < 0) {
            animTime += animationClip.length;
		}else{
			//if (animTime > animationClip.length) animTime = animationClip.length;
		}
        if (animTime < 0) animTime = 0;
        EditorGUILayout.LabelField("Animation Time:", animTime + " ("+ Mathf.Floor((animFrame/moveInfo.totalFrames) * 100) +"%)");

		Animator animator = targetChar.GetComponent<Animator>();
		if (animator == null) animator = (Animator) targetChar.AddComponent(typeof(Animator));
		if (animator.runtimeAnimatorController == null) 
			animator.runtimeAnimatorController = (RuntimeAnimatorController) Resources.Load("MC_Controller");
		
		if (loadHitBoxes){
			HitBoxesScript hitBoxesScript = targetChar.GetComponent<HitBoxesScript>();
			//hitBoxesScript.UpdateRenderer();

			// BEGIN INVERT ROTATION AND MIRROR OPTIONS
			if (invertRotation && !hitBoxesScript.previewInvertRotation){
				hitBoxesScript.previewInvertRotation = true;
				
				Vector3 rotationDirection = targetChar.transform.rotation.eulerAngles;
				rotationDirection.y += 180;
				targetChar.transform.rotation = Quaternion.Euler(rotationDirection);
			}else if (!invertRotation && hitBoxesScript.previewInvertRotation){
				hitBoxesScript.previewInvertRotation = false;
				
				Vector3 rotationDirection = targetChar.transform.rotation.eulerAngles;
				rotationDirection.y -= 180;
				targetChar.transform.rotation = Quaternion.Euler(rotationDirection);
			}

			if (hitBoxesScript.previewMirror != mirror){
				hitBoxesScript.previewMirror = mirror;
				targetChar.transform.localScale = new Vector3(-targetChar.transform.localScale.x, targetChar.transform.localScale.y, targetChar.transform.localScale.z);
			}
			// END INVERT ROTATION AND MIRROR OPTIONS

			if (loadHurtBoxes){
				foreach (Hit hit in moveInfo.hits){
					if (animFrame >= hit.activeFramesBegin && animFrame <= hit.activeFramesEnds) {
						if (hit.hurtBoxes.Length > 0){
							hitBoxesScript.activeHurtBoxes = hit.hurtBoxes;
						}
						break;
					}else{
						hitBoxesScript.activeHurtBoxes = null;
					}
				}
            }

			if (animFrame == 0 || animFrame == moveInfo.totalFrames) hitBoxesScript.HideHitBoxes(false);
		}
		if (animFrame > castingFrame) animationClip.SampleAnimation(targetChar, animTime);

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

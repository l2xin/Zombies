using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 刷新点包括刷新点、刷新范围、刷新数量和刷新间隔4个参数。							
/// SceneManager
/// </summary>
public class SceneAssetsManager : SingletonInstance<SceneAssetsManager>
{
    public static readonly ulong roleStartId = 1000;
	public static readonly ulong callNpcStartId = 10000;

    private GameObject aStarInitor;
    private AstarPath path;

    private static Vector3 generatorPos = Vector3.zero;
    private static float generatorRange = 3f;
    private static float generatorCount = 1f;
	private static float generatorCountPerframe = 1f;
    private static float generatorGap = 3f;
    private static int currentGeneratorWave = 0;
    private List<Player> currentWaveEnemys;
	private List<Player> callNpcs;
	private List<SceneItem> sceneItemList;
	private static ulong callNpcIndex = 0;

	private float totalGeneratorCount;
	private float currentGeneratorCount;

	private static float generatorItemRange = 0.3f;
	private static int maxGeneratorItemCount = 3;
	private bool isOpenGeneratorEnemys = false;
    private bool isOpenGeneratorItems = false;
    private GameObject enemySpawnContainer;
    private List<Vector3> enemySpawnList;

    public void Setup ()
    {
        aStarInitor = new GameObject();
        GameObject.DontDestroyOnLoad(aStarInitor);
        aStarInitor.name = "AStarInitor";
        path = aStarInitor.AddComponent<AstarPath>();
        path.logPathResults = PathLog.None;
        currentWaveEnemys = new List<Player>();
		sceneItemList = new List<SceneItem> ();
		callNpcs = new List<Player> ();
		SetInterval.Start (AutoGeneratorSceneItem, 1f, true);
        enemySpawnContainer = GameObject.Find("EnemySpawnContainer");
        enemySpawnList = new List<Vector3>();
        EnemySpawn[] enemySpawnVec = enemySpawnContainer.GetComponentsInChildren<EnemySpawn>();
        for (int i = 0; i < enemySpawnVec.Length; i++)
        {
            EnemySpawn enemySpawn = enemySpawnVec[i];
            enemySpawnList.Add(enemySpawn.transform.position);
        }
    }

	public void Clear()
	{
		SetInterval.Clear (AutoGeneratorSceneItem);
		SetTimeout.Clear (GeneratorNewWaveEnemysBy);
		if (currentWaveEnemys != null) {
			currentWaveEnemys.Clear ();
		}
		if (callNpcs != null) {
			callNpcs.Clear ();
		}
		if (sceneItemList != null) {
			sceneItemList.Clear ();
		}
	}

	private void AutoGeneratorSceneItem()
	{
        if (isOpenGeneratorItems)
        {
            if (FightManager.hero != null)
            {
                generatorItemRange = 3f;
                maxGeneratorItemCount = 2;
                GeneratorSceneItemsBy(FightManager.hero.transform.position);
            }
        }
	}

	public Player GetNearestEnemy()
	{
		float targetDistance = float.MaxValue;
		Player targetPlayer = null;
		for (int i = 0; i < currentWaveEnemys.Count; i++) {
			Player player = currentWaveEnemys[i];
			if (player.Physics != null && 
				player.isDead == false && 
				player.Physics.roleAIPath != null) {
				if (player.Physics.roleAIPath.targetDistance < 1) {
					return player;
				}
				else if (player.Physics.roleAIPath.targetDistance <= targetDistance) {
					targetPlayer = player;
				}
			}
		}
		return targetPlayer;
	}

    public void LoadMapDataFromCache()
    {
        Object bytes = Resources.Load("AstarPath/newGraph");
        TextAsset text = bytes as TextAsset;
        path.astarData.file_cachedStartup = text;
        path.astarData.LoadFromCache();
    }

    public void GeneratorNewWaveEnemys()
    {
		if (isOpenGeneratorEnemys) {
			currentWaveEnemys.Clear();
			totalGeneratorCount = generatorCount / generatorCountPerframe;
			currentGeneratorCount = 0;
			SetTimeout.Start(GeneratorNewWaveEnemysBy, generatorGap + currentGeneratorCount);
		}
    }

    public void TryGeneratorNewWaveEnemys()
    {
        for (int i = 0; i < currentWaveEnemys.Count; i++)
        {
            Player player = currentWaveEnemys[i];
            if (player.myInfo.isDead == false)
            {
                return;
            }
        }
		if (isOpenGeneratorEnemys) {
			GeneratorNewWaveEnemys();
		}
    }

    private void GeneratorNewWaveEnemysBy()
    {
        currentGeneratorWave++;
        int randomIndex = UnityEngine.Random.Range(0, enemySpawnList.Count);
        Vector3 enemySpawnPoint = enemySpawnList[randomIndex];
		for (int i = 0; i < generatorCountPerframe; i++)
        {
            Vector2 randomPoint = UnityEngine.Random.insideUnitCircle * generatorRange;
            Vector3 randomPointOfV3 = new Vector3(randomPoint.x, 0, randomPoint.y);
            enemySpawnPoint += randomPointOfV3;
            ulong id = (ulong)currentGeneratorWave * roleStartId + (ulong)(i + 1);
            List<object> objectList = ConfigManager.Instance.GetRandomName();
            string name = (string)objectList[objectList.Count - 1];
			MsgPlayer player = GeneratorMsgPlayer(enemySpawnPoint.x, enemySpawnPoint.z, name, id, UFECamp.Camp2);
            FightManager.playerModelMap.Add(player.id, player);
            Player enemy = FightManager.AddNetPlayer(player.id, (UFECamp)player.camp, UFEMapUnit.monster, (int)player.View);
            currentWaveEnemys.Add(enemy);
        }
		currentGeneratorCount++;
		if (currentGeneratorCount < totalGeneratorCount) {
			SetTimeout.Start(GeneratorNewWaveEnemysBy, generatorGap + currentGeneratorCount);
		}
    }
		
	public void GeneratorFollowNpc(Vector3 position, UFECamp camp, CharacterInfo characterInfo)
	{
		Vector2 randomPoint = UnityEngine.Random.insideUnitCircle * 0.3f;
		Vector3 randomPointOfV3 = new Vector3(randomPoint.x, 0, randomPoint.y);
		position += randomPointOfV3;
		ulong id = callNpcStartId + callNpcIndex;
		string name = "召唤物";
		MsgPlayer player = GeneratorMsgPlayer(position.x, position.z, name, id, camp);
		FightManager.playerModelMap.Add(player.id, player);
		Player callNpc = FightManager.AddNetPlayer(player.id, (UFECamp)player.camp, UFEMapUnit.monster, (int)player.View, true, characterInfo);
		callNpcs.Add (callNpc);
		callNpcIndex++;
	}

	public void GeneratorSceneItems(Vector3 position)
	{
		generatorItemRange = 0.3f;
		maxGeneratorItemCount = 1;
		GeneratorSceneItemsBy (position);
	}

	private void GeneratorSceneItemsBy(Vector3 position)
	{
		int maxCount =  UnityEngine.Random.Range(0, maxGeneratorItemCount);
		for (int i = 0; i < maxCount; i++) {
			bool isGeneratorWeapon = UnityEngine.Random.Range (0, 1) >= 0.99f ? true : false;
			SceneItem sceneItem;
			if (isGeneratorWeapon) {
				int weaponIndex = -1;
				WeaponType weaponType = UnityEngine.Random.Range(0, 1) >= 0.5f ? WeaponType.Range : WeaponType.Melee;
				if (weaponType == WeaponType.Melee) {
					weaponIndex = UnityEngine.Random.Range (0, FightManager.config.meleeWeapons.Length - 1);
				} else if (weaponType == WeaponType.Range) {
					weaponIndex = UnityEngine.Random.Range (0, FightManager.config.bows.Length - 1);
				}
				sceneItem = GetItem (weaponType, weaponIndex);
			} else {
				sceneItem = GetItem (WeaponType.Melee, -1);
			}

			sceneItem.transform.parent = FightManager.gameEngine.transform;
			Vector2 randomPoint = UnityEngine.Random.insideUnitCircle * generatorItemRange;
			Vector3 randomPointOfV3 = new Vector3(randomPoint.x, 0.5f, randomPoint.y);
			sceneItem.transform.position = position + randomPointOfV3;
		}
	}

	public void GeneratorSceneItems(Vector3 position, WeaponType weaponType, int weaponIndex = -1)
	{
		int maxCount = UnityEngine.Random.Range(0, maxGeneratorItemCount);
		for (int i = 0; i < maxCount; i++) {
			SceneItem sceneItem = GetItem (weaponType, weaponIndex);
			sceneItem.transform.parent = FightManager.gameEngine.transform;
			sceneItem.transform.position = position;
			sceneItemList.Add (sceneItem);
		}
	}

	private SceneItem GetItem(WeaponType weaponType, int weaponIndex = -1)
	{
		GameObject item = null;
		GameObject itemPrefab = null;
		if (weaponIndex >= 0) {
			switch ((WeaponType)weaponType)
			{
			case WeaponType.Melee:
				itemPrefab = FightManager.config.meleeWeapons [weaponIndex];
				item = PoolUtil.SpawnerGameObject (itemPrefab, PoolUtil.particlesPoolName);
				break;
			case WeaponType.Range:
				itemPrefab = FightManager.config.bows [weaponIndex];
				item = PoolUtil.SpawnerGameObject (itemPrefab, PoolUtil.particlesPoolName);
				break;
			default:
				break;
			}
		} else {
			itemPrefab = FightManager.config.roundOptions.callBossItem;
			item = PoolUtil.SpawnerGameObject (itemPrefab, PoolUtil.particlesPoolName);
		}
		SceneItem currentItem = item.GetComponent<SceneItem> ();
		if(currentItem == null)
		{
			currentItem = item.AddComponent<SceneItem> ();	
		}
		if (weaponIndex >= 0) {
			currentItem.weaponType = weaponType;
			currentItem.weaponIndex = weaponIndex;
		}
		return currentItem;
	}

	private MsgPlayer GeneratorMsgPlayer(float spawnX, float spawnZ, string name, ulong id, UFECamp camp)
    {
        MsgPlayer role = new MsgPlayer();
        role.Realposx = spawnX;
        role.Realposz = spawnZ;
        role.name = name;
        role.id = id;
		role.camp = (uint)camp;
        return role;
    }
}

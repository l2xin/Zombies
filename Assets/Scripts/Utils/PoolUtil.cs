using UnityEngine;
using PathologicalGames;
using System.Collections.Generic;
using System.IO;

public class PoolUtil
{
    public const string particlesPoolName = "Particles";
    public const string guiPoolName = "GUI";
    private const int maxSpawnerInputBuffer = 30;
    private const int maxSpawnerInputEvents = 50;
    private static List<InputEvents> cacheListInputEvents = new List<InputEvents>();
    private static List<MemoryStream> cacheListMemoryStream = new List<MemoryStream>();
    private static List<BinaryReader> cacheListBinaryReader = new List<BinaryReader>();
    private static List<MoveInfo> cacheListMoveInfo = new List<MoveInfo>();

    private static List<Dictionary<InputReferences, InputEvents>> cacheListInputBuffer = new List<Dictionary<InputReferences, InputEvents>>();
    private static Dictionary<string, List<MoveInfo>> cacheMoveInfoMap = new Dictionary<string, List<MoveInfo>>();
    private static List<ByteArray> cacheByteArrayList = new List<ByteArray>();


    public static ByteArray SpawnerByteArray()
    {
        if(cacheByteArrayList.Count > 0)
        {
            ByteArray byteArray = cacheByteArrayList[0];
            cacheByteArrayList.RemoveAt(0);
            byteArray.Clear();
            return byteArray;
        }
        return new ByteArray();
    }

    public static void DespawnerByteArray(ByteArray byteArray)
    {
        cacheByteArrayList.Add(byteArray);
    }

    public static void ClearMoveInfoMap()
    {
        cacheMoveInfoMap.Clear();
    }

    public static MoveInfo SpawnerMoveInfo(MoveInfo moveInfo)
    {
        if(cacheMoveInfoMap.ContainsKey(moveInfo.moveName))
        {
            List<MoveInfo> moveInfoList = cacheMoveInfoMap[moveInfo.moveName];
            if(moveInfoList.Count > 0)
            {
                MoveInfo cacheMoveInfo = moveInfoList[0];
                moveInfoList.RemoveAt(0);
                for (int i = 0; i < cacheMoveInfo.particleEffects.Length; i++)
                {
                    MoveParticleEffect moveParticleEffect = cacheMoveInfo.particleEffects[i];
                    moveParticleEffect.casted = false;
                }
                for (int i = 0; i < cacheMoveInfo.projectiles.Length; i++)
                {
                    Projectile projectile = cacheMoveInfo.projectiles[i];
                    projectile.casted = false;
                }
                for (int i = 0; i < cacheMoveInfo.soundEffects.Length; i++)
                {
                    SoundEffect soundEffect = cacheMoveInfo.soundEffects[i];
                    soundEffect.casted = false;
                }
                for (int i = 0; i < cacheMoveInfo.appliedForces.Length; i++)
                {
                    AppliedForce appliedForce = cacheMoveInfo.appliedForces[i];
                    appliedForce.casted = false;
                }
                for (int i = 0; i < cacheMoveInfo.frameLinks.Length; i++)
                {
                    FrameLink frameLink = cacheMoveInfo.frameLinks[i];
                    frameLink.cancelable = moveInfo.frameLinks[i].cancelable;
                }
				for (int i = 0; i < cacheMoveInfo.hits.Length; i++) {
					Hit hit = cacheMoveInfo.hits[i];
					hit.disabled = false;
				}
				for (int i = 0; i < cacheMoveInfo.callNpcs.Length; i++)
				{
					CallNpc callNpc = cacheMoveInfo.callNpcs[i];
					callNpc.casted = false;
				}
                return cacheMoveInfo;
            }
        }
        return GameObject.Instantiate(moveInfo) as MoveInfo;
    }

    public static void DespawnerMoveInfo(MoveInfo moveInfo)
    {
        if (cacheMoveInfoMap.ContainsKey(moveInfo.moveName))
        {
            List<MoveInfo> moveInfoList = cacheMoveInfoMap[moveInfo.moveName];
            moveInfoList.Add(moveInfo);
        }
        else
        {
            List<MoveInfo> moveInfoList = new List<MoveInfo>();
            moveInfoList.Add(moveInfo);
            cacheMoveInfoMap[moveInfo.moveName] = moveInfoList;
        }
    }

    public static void DespawnerInputBuffer(Dictionary<InputReferences, InputEvents> inputBuffer)
    {
        if(cacheListInputBuffer.IndexOf(inputBuffer) < 0 && cacheListInputBuffer.Count <= maxSpawnerInputBuffer)
        {
            cacheListInputBuffer.Add(inputBuffer);
        }
    }

    public static Dictionary<InputReferences, InputEvents> SpawnerInputBuffer()
    {
        if (cacheListInputBuffer.Count > 0)
        {
            Dictionary<InputReferences, InputEvents> inputBuffer = cacheListInputBuffer[0];
            cacheListInputBuffer.RemoveAt(0);
            return inputBuffer;
        }
        return new Dictionary<InputReferences, InputEvents>();
    }

    public static InputEvents SpawnerInputEvents(float axis, float axisRaw)
    {
        if (cacheListInputEvents.Count > 0)
        {
            InputEvents inputEvents = cacheListInputEvents[0];
            cacheListInputEvents.RemoveAt(0);
            inputEvents.axis = axis;
            inputEvents.axisRaw = axisRaw;
            inputEvents.button = axisRaw != 0;
            return inputEvents;
        }
        return new InputEvents(axis, axisRaw);
    }

    public static MemoryStream SpawnerMemoryStream(byte[] serializedNetworkMessage)
    {
        if (cacheListMemoryStream.Count > 0)
        {
            MemoryStream memoryStream = cacheListMemoryStream[0];
            memoryStream.Write(serializedNetworkMessage, 0, serializedNetworkMessage.Length);
            return memoryStream;
        }
        return new MemoryStream(serializedNetworkMessage);
    }

    public static BinaryReader SpawnerBinaryReader(MemoryStream stream)
    {
        if (cacheListBinaryReader.Count > 0)
        {
            BinaryReader binaryReader = cacheListBinaryReader[0];
            return binaryReader;
        }
        return new BinaryReader(stream);
    }

    public static InputEvents SpawnerInputEvents(bool button)
    {
        if (cacheListInputEvents.Count > 0)
        {
            InputEvents inputEvents = cacheListInputEvents[0];
            cacheListInputEvents.RemoveAt(0);
            inputEvents.button = button;
            inputEvents.axis = 0f;
            inputEvents.axisRaw = 0f;
            return inputEvents;
        }
        return new InputEvents(button);
    }

    public static InputEvents SpawnerInputEvents(InputEvents other)
    {
        if (cacheListInputEvents.Count > 0)
        {
            InputEvents inputEvents = cacheListInputEvents[0];
            cacheListInputEvents.RemoveAt(0);
            inputEvents.button = other.button;
            inputEvents.axis = other.axis;
            inputEvents.axisRaw = other.axisRaw;
            return inputEvents;
        }
        return new InputEvents(other);
    }

    public static void DespawnerInputEvents(InputEvents inputEvents)
    {
        if(cacheListInputEvents.IndexOf(inputEvents) < 0 && cacheListInputEvents.Count <= maxSpawnerInputEvents)
        {
            inputEvents.Reset();
            cacheListInputEvents.Add(inputEvents);
        }
    }

    public static void DespawnerBinaryReader(BinaryReader binaryReader)
    {
        cacheListBinaryReader.Add(binaryReader);
    }

    public static void DespawnerMemoryStream(MemoryStream memoryStream)
    {
        cacheListMemoryStream.Add(memoryStream);
    }

    public static Transform Spawner(GameObject trans, string poolName = particlesPoolName)
    {
        SpawnPool shapesPool = PoolManager.Pools[poolName];
        Transform instance = shapesPool.Spawn(trans);
        instance.gameObject.SetActive(true);
        return instance;
    }

    public static GameObject SpawnerGameObject(string transName, string poolName = particlesPoolName)
    {
        SpawnPool shapesPool = PoolManager.Pools[poolName];
        if(shapesPool.prefabs.ContainsKey(transName))
        {
            return shapesPool.Spawn(transName).gameObject;
        }
        else
        {
            GameObject prefab = new GameObject(transName);
            prefab.transform.parent = shapesPool.transform;
            StartLoad(transName, prefab);
            return prefab;
        }
    }

    private static void StartLoad(string transName, GameObject parent)
    {
        MainEntry.Instance.StartLoad(transName, AssetType.prefab, (GameObject effect, string tag) =>
        {
            SpawnPool shapesPool = PoolManager.Pools[particlesPoolName];
            effect.name = transName;
            effect.transform.parent = shapesPool.transform;
            effect.SetActive(false);
            GameObject go = PoolUtil.SpawnerGameObject(effect);
            if (parent != null)
            {
                go.transform.parent = parent.transform;
                go.transform.localPosition = Vector3.zero;
                go.transform.rotation = parent.transform.rotation;
            }
            else
            {
                PoolUtil.Despawner(go);
            }
        });
    }

    public static GameObject SpawnerGameObject(GameObject trans, string poolName = particlesPoolName, bool cullDespawned = false, int cullAbove = 50, int cullDelay = 60, int cullMaxPerPass = 5)
    {
        if(trans != null)
        {
            SpawnPool shapesPool = PoolManager.Pools[poolName];
            Transform instance = shapesPool.Spawn(trans);
            instance.gameObject.SetActive(true);

            if(cullDespawned)
            {
                PrefabPool prefabPool = shapesPool.GetPrefabPool(trans);
                if (prefabPool != null)
                {
                    prefabPool.cullDespawned = cullDespawned;
                    prefabPool.cullAbove = cullAbove;
                    prefabPool.cullDelay = cullDelay;
                    prefabPool.cullMaxPerPass = cullMaxPerPass;
                }
            }

            return instance.gameObject;
        }
        return null;
    }

    public static int GetCacheParticlesCount()
    {
        SpawnPool shapesPool = PoolManager.Pools[particlesPoolName];
        if(shapesPool != null)
        {
            return shapesPool.transform.childCount;
        }
        return 0;
    }

    public static void DespawnAll(string poolName = particlesPoolName)
    {
        SpawnPool shapesPool = PoolManager.Pools[poolName];
        shapesPool.DespawnAll();
    }

    public static GameObject SpawnerGameObject(GameObject trans, float delay, string poolName = particlesPoolName)
    {
        SpawnPool shapesPool = PoolManager.Pools[poolName];
        Transform instance = shapesPool.Spawn(trans);
        instance.gameObject.SetActive(true);
        if (delay > 0)
        {
            shapesPool.Despawn(instance, delay, shapesPool.transform);
        }
        return instance.gameObject;
    }

    public static bool Despawner(GameObject trans, string poolName = particlesPoolName, bool isNeedCheck = false)
    {
        if (trans != null)
        {
            if (isNeedCheck && trans.transform.childCount <= 0)
            {
                GameObject.Destroy(trans);
                return false;
            }
            else
            {
                SpawnPool shapesPool = PoolManager.Pools[poolName];
                if (poolName == guiPoolName)
                {
                    trans.transform.SetParent(shapesPool.transform);
                }
                else
                {
                    trans.transform.parent = shapesPool.transform;
                }
                bool isSucess = shapesPool.Despawn(trans.transform);
                if (isSucess == false)
                {
                    GameObject.DestroyImmediate(trans, true);
                } 
            }
        }
        return false;
    }
}

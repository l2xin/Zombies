using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// ProjectileMoveScript
/// 抛射物
/// nashnie
/// </summary>
public class ProjectileMoveScript : MonoBehaviour
{
    public ulong ownerId;
    private string ownerTag;
    private Projectile data;

    public Player myControlsScript;
    public HurtBox hurtBox;
    public HitBox hitBox;

    private bool isDestroyed = false;

    public Hit hit;
    private Vector3 directionVector = new Vector3(1, 0, 0);

    private float isHit;
    private float spaceBetweenHits = .1f;

    private Vector3 movement;
    private Renderer projectileRenderer;
    private Bounds bounds;

    public int id;
    public int skillId;

    private ProjectileMovement projectileMovement;

    private int usecond = 1000000;

    private bool isColliderTrigger = false;
    private bool isCheckCollider = false;
    private BoxCollider boxCollider;
    private LayerMask layerMask;
    private int groundLayer = 8;
    private LineRenderer lineRenderer;
    private Transform startBodyPart;
    private int totalHits = 1;
    private bool isHitSpace = false;

    public Projectile Data
    {
        get
        {
            return data;
        }
        set
        {
            if (data != value)
            {
                data = value;
                ownerTag = myControlsScript.tag;
                gameObject.SetActive(true);
                boxCollider = gameObject.GetComponent<BoxCollider>();
                layerMask = 1 << groundLayer;

                startBodyPart = myControlsScript.myHitBoxesScript.GetTransform(data.bodyPart);
                data.startPos = startBodyPart.position;
                data.skillId = (uint)skillId;

                data.directionVec.x = myControlsScript.rotDirection.x;
                data.directionVec.z = myControlsScript.rotDirection.y;

                if (data.directionAngle != 0)
                {
                    if (data.moveableType == ProjectileMoveableType.normal)
                    {
                        if (data.movementType == ProjectileMovementUtil.MovementType.line)
                        {
                            data.directionVec = Quaternion.Euler(0, data.directionAngle, 0) * data.directionVec;
                        }
                        else if (data.movementType == ProjectileMovementUtil.MovementType.parabola)
                        {
                            data.directionVec = Quaternion.Euler(-data.directionAngle, 0, 0) * data.directionVec;
                        }
                    }
                }

                transform.localScale = Vector3.one;
                transform.position = data.startPos;
                transform.parent = FightManager.gameEngine.transform;
                ParseMovement();
            }
        }
    }

    public void DestroyGameObject()
    {
        SetTimeout.Clear(DestroyGameObject);
        SetTimeout.Clear(SetProjectileScale);
        //SetTimeout.Start(FireRemoveProjectileHandler, 0.5f);
        FireRemoveProjectileHandler();
        if (isDestroyed == false)
        {
            iTween.Stop(gameObject);
            FightManager.RemoveDelaySynchronizedAction(onViewComplete);
            PoolUtil.Despawner(gameObject, PoolUtil.particlesPoolName, true);
            isDestroyed = true;
        }
    }

    private void FireRemoveProjectileHandler()
    {
        GameObject.Destroy(this);
        data = null;
        hit = null;
        projectileMovement = null;
        myControlsScript = null;
        FightManager.FireRemoveProjectileHandler(this);
    }

    public void DestroyGameObjectByFightEnd()
    {
        SetTimeout.Clear(DestroyGameObject);
        SetTimeout.Clear(SetProjectileScale);
        //SetTimeout.Clear(FireRemoveProjectileHandler);
        if (isDestroyed == false)
        {
            hit = null;
            data = null;
            projectileMovement = null;
            myControlsScript = null;
            GameObject.Destroy(this);
            iTween.Stop(gameObject);
            FightManager.RemoveDelaySynchronizedAction(onViewComplete);
            PoolUtil.Despawner(gameObject, PoolUtil.particlesPoolName, true);
            isDestroyed = true;
        }
    }

    private class ProjectileMovement
    {
        public int id;
        public ulong playerId;
        public short playerType;
        public int skillid;
        public uint pathMovementType;       // 弹道类型
        public uint frameCount;             // 运行帧数
        public float backMul = 1;           // 回弹系数
        public bool backAutoFlag;
        public float fixedDeltaTimeMoveDistance;
        public Vector3 fixedDeltaTimeMoveMotion;

        public float lastFoldTime;      // 上一次折走的时间(ms)
        public Vector3 flySpdVec;       // 飞行速度方向分量
        public Vector3 posVec;          // 技能抵达的位置
        public Vector3 prePosVec;       // 上一次抵达的位置
        public Vector3 oriPosVec;       // 起飞点
        public Vector3 directionVec;
        public float time;              // 要飞行的时长(ms)
        public float speed;             // 飞行速度
        public float circleRadius; 	    // 子弹半径
        public float nextFrameExTime = 0f;
        public float foldSingleLineMoveTime = 0f;

        public bool IsProjectileMoveDone
        {
            get
            {
                return frameCount * Time.fixedDeltaTime > time;
            }
        }
    }

    private void AddMotion()
    {
        projectileMovement.prePosVec = projectileMovement.posVec;
        projectileMovement.frameCount = projectileMovement.frameCount + 1;

        switch (projectileMovement.pathMovementType)
        {
            case (uint)ProjectileMovementUtil.MovementType.circle:
                //围绕人物旋转
                float f = (float)(projectileMovement.frameCount * projectileMovement.fixedDeltaTimeMoveDistance) / ProjectileMovementUtil.circleRadious;
                projectileMovement.posVec.x = myControlsScript.transform.position.x + ProjectileMovementUtil.circleRadious * Mathf.Cos(f);
                projectileMovement.posVec.z = myControlsScript.transform.position.z + ProjectileMovementUtil.circleRadious * Mathf.Sin(f);
                break;
            case (uint)ProjectileMovementUtil.MovementType.boomerang:
                // 原路返回
                projectileMovement.posVec.x = projectileMovement.posVec.x + projectileMovement.fixedDeltaTimeMoveMotion.x * projectileMovement.backMul;
                projectileMovement.posVec.z = projectileMovement.posVec.z + projectileMovement.fixedDeltaTimeMoveMotion.z * projectileMovement.backMul;
                break;
            case (uint)ProjectileMovementUtil.MovementType.sinLine:
                // 正弦曲线
                Vector3 crossDirection = new Vector3(projectileMovement.directionVec.z, 0, -projectileMovement.directionVec.x);
                float time = projectileMovement.frameCount * Time.fixedDeltaTime;
                Vector3 addMotion = projectileMovement.directionVec * projectileMovement.frameCount * projectileMovement.fixedDeltaTimeMoveDistance;
                Vector3 addSinMotion = crossDirection * Mathf.Sin(time * ProjectileMovementUtil.frequency) * ProjectileMovementUtil.amplitude;
                projectileMovement.posVec = projectileMovement.oriPosVec + addMotion + addSinMotion;
                break;
            case (uint)ProjectileMovementUtil.MovementType.foldLine:
                //折线
                bool isHasExTime = false;
                bool isChangeDirectionVec = false;
                float exTime = 0f;
                if (projectileMovement.foldSingleLineMoveTime <= Time.realtimeSinceStartup * usecond - projectileMovement.lastFoldTime)
                {
                    exTime = Time.realtimeSinceStartup * usecond - projectileMovement.lastFoldTime - projectileMovement.foldSingleLineMoveTime;
                    isHasExTime = exTime > 0;
                    isChangeDirectionVec = true;
                    projectileMovement.lastFoldTime = Time.realtimeSinceStartup * usecond;
                }
                if (projectileMovement.nextFrameExTime > 0)
                {
                    projectileMovement.posVec += projectileMovement.directionVec * projectileMovement.speed * (Time.fixedDeltaTime + projectileMovement.nextFrameExTime / usecond);
                    projectileMovement.nextFrameExTime = 0;
                }
                else if (isHasExTime)
                {
                    projectileMovement.nextFrameExTime = exTime;
                    projectileMovement.posVec += projectileMovement.directionVec * projectileMovement.speed * (Time.fixedDeltaTime - projectileMovement.nextFrameExTime / usecond);
                }
                else
                {
                    projectileMovement.posVec += projectileMovement.directionVec * projectileMovement.speed * Time.fixedDeltaTime;
                }
                if (isChangeDirectionVec)
                {
                    projectileMovement.oriPosVec = projectileMovement.posVec;
                    projectileMovement.backMul = projectileMovement.backMul * -1.0f;
                    if (projectileMovement.backMul == 1.0)
                    {
                        projectileMovement.directionVec = PerpUp(projectileMovement.directionVec);
                    }
                    else
                    {
                        projectileMovement.directionVec = PerpDown(projectileMovement.directionVec);
                    }
                }
                break;
            case (uint)ProjectileMovementUtil.MovementType.parabola:
                projectileMovement.posVec += projectileMovement.flySpdVec * Time.fixedDeltaTime;
                projectileMovement.flySpdVec.y -= data.gravity * Time.fixedDeltaTime;
                break;
            case (uint)ProjectileMovementUtil.MovementType.line:
                projectileMovement.posVec += projectileMovement.fixedDeltaTimeMoveMotion;
                break;
            default:
                break;
        }

        if (isCheckCollider)
        {
            switch (data.throughWallType)
            {
                case ProjectileMovementUtil.ThroughType.bounce:
                case ProjectileMovementUtil.ThroughType.collider:
                    RaycastHit hit;
                    float maxDistance = Vector3.Distance(projectileMovement.prePosVec, projectileMovement.posVec);
                    if (Physics.Raycast(projectileMovement.prePosVec, directionVector, out hit, maxDistance, layerMask))
                    {
                        if (data.throughWallType == ProjectileMovementUtil.ThroughType.bounce)
                        {
                            directionVector = Vector3.Reflect(directionVector, hit.normal);
                            projectileMovement.directionVec = directionVector;
                            projectileMovement.flySpdVec = projectileMovement.speed * directionVector;
                            projectileMovement.posVec = hit.point;
                            projectileMovement.fixedDeltaTimeMoveMotion = projectileMovement.flySpdVec * Time.fixedDeltaTime;
                        }
                        else if (data.throughWallType == ProjectileMovementUtil.ThroughType.collider)
                        {
                            isColliderTrigger = true;
                            float impactDuration = data.impactDuration > 0 ? data.impactDuration : 1f;
                            SetTimeout.Start(DestroyGameObject, impactDuration);
                            projectileMovement.posVec = hit.point;
                        }
                    }
                    break;
                case ProjectileMovementUtil.ThroughType.through:
                default:
                    break;
            }
        }
    }

    private Vector3 PerpUp(Vector3 v)
    {
        return new Vector3(-v.z, v.y, v.x);
    }

    private Vector3 PerpDown(Vector3 v)
    {
        return new Vector3(v.z, v.y, -v.x);
    }

    private void FixedUpdate()
    {
        if (projectileMovement != null && isColliderTrigger == false && isDestroyed == false)
        {
            if (projectileMovement.IsProjectileMoveDone)
            {
                if (data.impactPrefab != null)
                {
                    GameObject impactEffect = PoolUtil.SpawnerGameObject(data.impactPrefab);
                    projectileMovement.posVec.y = 0;
                    impactEffect.transform.position = projectileMovement.posVec;
                    impactEffect.transform.rotation = transform.rotation;
                    impactEffect.transform.parent = transform.parent;
                    float impactDuration = data.impactDuration > 0 ? data.impactDuration : 1f;
                    DestroyObject(impactEffect, impactDuration);
                }
                DestroyGameObject();
            }
            else
            {
                AddMotion();
                if (data.selfRotationSpeed > 0)
                {
                    transform.Rotate(0, data.selfRotationSpeed, 0);
                }
                else
                {
                    transform.LookAt(projectileMovement.posVec);
                }
                transform.transform.position = projectileMovement.posVec;

#if UNITY_EDITOR
                /*if (Global.isShowProjectilePath)
                {
                    GameObject followSphereForTest = GameObject.Instantiate(UIManager.followSphere, transform.position, transform.rotation) as GameObject;
                    followSphereForTest.transform.localScale = Vector3.one * 0.1f;
                    DestroyObject(followSphereForTest, 5f);
                }*/
#endif  

                if (projectileMovement.pathMovementType == (uint)ProjectileMovementUtil.MovementType.boomerang &&
                    projectileMovement.frameCount * Time.fixedDeltaTime >= projectileMovement.time / 2f &&
                    projectileMovement.backAutoFlag == false)
                {
                    projectileMovement.backMul = projectileMovement.backMul * -1.0f;
                    projectileMovement.backAutoFlag = true;
                    return;
                }
            }
        }
        else if (data.moveableType == ProjectileMoveableType.laser)
        {
            if(myControlsScript != null)
            {
                if (myControlsScript.IsContinueNormalAttack == false)
                {
                    lineRenderer.enabled = false;
                    DestroyGameObject();
                }
                else
                {
                    data.directionVec.x = myControlsScript.rotDirection.x;
                    data.directionVec.z = myControlsScript.rotDirection.y;
                    hit.pushForceDirection = data.directionVec;
                    Ray ray = new Ray(startBodyPart.position, data.directionVec);
                    RaycastHit raycastHit;
                    lineRenderer.SetPosition(0, ray.origin);
                    if (Physics.Raycast(ray, out raycastHit, data.range))
                    {
                        lineRenderer.SetPosition(1, raycastHit.point);
                        OnTriggerEnter(raycastHit.collider);
                    }
                    else
                    {
                        lineRenderer.SetPosition(1, ray.GetPoint(data.range));
                    }
                }
            }
            else
            {
                DestroyGameObject();
            }
        }
    }

    private void ParseMovement()
    {
        float speed = data.speed;
        float duration = data.duration;

        directionVector = data.directionVec;
        movement = speed * directionVector;
        transform.LookAt(transform.position + movement);

        if (data.moveableType == ProjectileMoveableType.normal)
        {
            iTween.ScaleTo(gameObject, Vector3.zero, 0f);
            SetTimeout.Start(SetProjectileScale, 0.1f);

            projectileMovement = new ProjectileMovement();
            projectileMovement.playerId = myControlsScript.id;
            projectileMovement.time = duration;
            projectileMovement.pathMovementType = (uint)data.movementType;
            projectileMovement.speed = speed;
            projectileMovement.id = id;

            projectileMovement.flySpdVec = speed * directionVector;
            projectileMovement.directionVec = directionVector;
            projectileMovement.oriPosVec = transform.position;
            projectileMovement.posVec = transform.position;
            projectileMovement.fixedDeltaTimeMoveDistance = speed * Time.fixedDeltaTime;
            projectileMovement.fixedDeltaTimeMoveMotion = projectileMovement.flySpdVec * Time.fixedDeltaTime;

            if (data.movementType == ProjectileMovementUtil.MovementType.foldLine)
            {
                float x = projectileMovement.directionVec.x;
                float z = projectileMovement.directionVec.z;
                float defaultRate = 0.707f;
                projectileMovement.directionVec.x = x * defaultRate - z * defaultRate;
                projectileMovement.directionVec.z = x * defaultRate + z * defaultRate;
                projectileMovement.directionVec.Normalize();
                projectileMovement.prePosVec = transform.position;
                projectileMovement.lastFoldTime = usecond * (Time.realtimeSinceStartup - ((ProjectileMovementUtil.foldLineLen / 2) / projectileMovement.speed));
                projectileMovement.foldSingleLineMoveTime = usecond * ProjectileMovementUtil.foldLineLen / projectileMovement.speed;
            }
            else if (data.movementType == ProjectileMovementUtil.MovementType.boomerang)
            {
                projectileMovement.time *= 2f;
            }
        }
        else if (data.moveableType == ProjectileMoveableType.laser)
        {
            lineRenderer = gameObject.GetComponent<LineRenderer>();
            Ray ray = new Ray(transform.position, data.directionVec);
            RaycastHit raycastHit;
            lineRenderer.SetPosition(0, ray.origin);
            if(Physics.Raycast(ray, out raycastHit, data.range))
            {
                lineRenderer.SetPosition(1, raycastHit.point);
                OnTriggerEnter(raycastHit.collider);
            }
            else
            {
                lineRenderer.SetPosition(1, ray.GetPoint(data.range));
            }
            FightManager.DelaySynchronizedAction(onViewComplete, duration);
        }
        else if(data.moveableType == ProjectileMoveableType.boxWithoutMove)
        {
            transform.localScale = new Vector3(data.rangeOfV2.x, 0.002f, data.rangeOfV2.y);
            transform.position += transform.forward * data.rangeOfV2.y / 2;
            //TODO 调整 Time.fixedDeltaTime * 3
            FightManager.DelaySynchronizedAction(onViewComplete, Time.fixedDeltaTime * 3);
        }

        //TODO cache
        hit = new Hit();
        hit.hitStrength = data.hitStrength;
        hit.overrideHitEffects = data.overrideHitEffects;
        hit.overrideHitAnimation = data.overrideHitAnimation;
        hit.newHitAnimation = data.newHitAnimation;
        hit.hitEffects = data.hitEffects;
        hit.pushForce = data.pushForce;
        hit.pushForceTime = data.pushForceTime;
        hit.pushForceDirection = directionVector;
        hit.debuffType = data.debuffType;
        hit.debuffParam = data.debuffParam;
        hit.spaceBetweenHits = data.spaceBetweenHits;
        //hit.hitType = data.hitType;

        if (hit.spaceBetweenHits == Sizes.Small)
        {
            spaceBetweenHits = .1f;
            totalHits = Mathf.CeilToInt(duration / spaceBetweenHits);
        }
        else if (hit.spaceBetweenHits == Sizes.Medium)
        {
            spaceBetweenHits = .3f;
            totalHits = Mathf.CeilToInt(duration / spaceBetweenHits);
        }
        else if (hit.spaceBetweenHits == Sizes.High)
        {
            spaceBetweenHits = .5f;
            totalHits = Mathf.CeilToInt(duration / spaceBetweenHits);
        }
        else
        {
            spaceBetweenHits = 0;
            totalHits = 1;
        }
    }

    private void SetProjectileScale()
    {
        isCheckCollider = true;
        iTween.ScaleTo(gameObject, Vector3.one, 0.3f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (totalHits > 1 && isHitSpace)
        {
            return;
        }
        if (other.CompareTag(ownerTag) == false && 
            (other.CompareTag(FightManager.EnemyTag) || other.CompareTag(FightManager.PlayerTag)))
        {
            Player enemy = other.gameObject.GetComponent<Player>();
            uint hpDec = 1;
            enemy.GetHit(hit, hpDec, myControlsScript);

            if (spaceBetweenHits > 0)
            {
                FightManager.DelaySynchronizedAction(ResetHit, spaceBetweenHits);
                isHitSpace = true;
                if (boxCollider != null)
                {
                    boxCollider.enabled = false;
                }
            }
            totalHits--;
            if (data.throughEnemyType == ProjectileMovementUtil.ThroughType.collider && totalHits <= 0)
            {
                DestroyGameObject();
            }
        }
    }

    private void ResetHit()
    {
        isHitSpace = false;
        if (boxCollider != null)
        {
            boxCollider.enabled = true;
        }
    }

    private void onViewComplete()
    {
        DestroyGameObject();
    }

    public void UpdateRenderer()
    {
        if (hurtBox.followXBounds || hurtBox.followYBounds)
        {
            Renderer[] rendererList = GetComponentsInChildren<Renderer>();
            for (int i = 0; i < rendererList.Length; i++)
            {
                Renderer childRenderer = rendererList[i];
                projectileRenderer = childRenderer;
            }
            if (projectileRenderer == null)
            {
                Debug.LogWarning("Warning: You are trying to access the projectile's bounds, but it does not have a renderer.");
            }
        }
    }

    void OnDestroy()
    {
        FightManager.RemoveDelaySynchronizedAction(onViewComplete);
    }

    public bool IsDestroyed()
    {
        if (this == null)
        {
            return true;
        }
        if (isDestroyed)
        {
            return true;
        }
        return false;
    }

    public Rect GetBounds()
    {
        if (projectileRenderer != null)
        {
            return new Rect(projectileRenderer.bounds.min.x,
                            projectileRenderer.bounds.min.y,
                            projectileRenderer.bounds.max.x,
                            projectileRenderer.bounds.max.y);
        }

        return new Rect();
    }

    private void GizmosDrawRectangle(Vector3 topLeft, Vector3 bottomLeft, Vector3 bottomRight, Vector3 topRight)
    {
        Gizmos.DrawLine(topLeft, bottomLeft);
        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
    }

    void OnDrawGizmos()
    {
        if (hurtBox != null)
        {
            Gizmos.color = Color.cyan;

            Vector3 hurtBoxPosition = transform.position;
            if (FightManager.config == null || !FightManager.config.detect3D_Hits) hurtBoxPosition.z = -1;

            if (hurtBox.shape == HitBoxShape.circle)
            {
                hurtBoxPosition += new Vector3(hurtBox.offSet.x * -1, hurtBox.offSet.y, 0);
                Gizmos.DrawWireSphere(hurtBoxPosition, hurtBox.radius);
            }
            else
            {
                Vector3 topLeft = new Vector3(hurtBox.rect.x * -1, hurtBox.rect.y) + hurtBoxPosition;
                Vector3 topRight = new Vector3((hurtBox.rect.x + hurtBox.rect.width) * -1, hurtBox.rect.y) + hurtBoxPosition;
                Vector3 bottomLeft = new Vector3(hurtBox.rect.x * -1, hurtBox.rect.y + hurtBox.rect.height) + hurtBoxPosition;
                Vector3 bottomRight = new Vector3((hurtBox.rect.x + hurtBox.rect.width) * -1, hurtBox.rect.y + hurtBox.rect.height) + hurtBoxPosition;

                if (hurtBox.followXBounds)
                {
                    hurtBox.rect.x = 0;
                    topLeft.x = GetBounds().x - (hurtBox.rect.width / 2);
                    topRight.x = GetBounds().width + (hurtBox.rect.width / 2);
                    bottomLeft.x = GetBounds().x - (hurtBox.rect.width / 2);
                    bottomRight.x = GetBounds().width + (hurtBox.rect.width / 2);
                }

                if (hurtBox.followYBounds)
                {
                    hurtBox.rect.y = 0;
                    topLeft.y = GetBounds().height + (hurtBox.rect.height / 2);
                    topRight.y = GetBounds().height + (hurtBox.rect.height / 2);
                    bottomLeft.y = GetBounds().y - (hurtBox.rect.height / 2);
                    bottomRight.y = GetBounds().y - (hurtBox.rect.height / 2);
                }
                GizmosDrawRectangle(topLeft, bottomLeft, bottomRight, topRight);
            }
        }

        Gizmos.color = Color.red;
    }
}

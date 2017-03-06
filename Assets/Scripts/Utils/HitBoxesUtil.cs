using UnityEngine;
using System.Collections.Generic;

public class HitBoxesUtil : MonoBehaviour
{
    public HitBox[] hitBoxes;

    [HideInInspector]
    public bool isHit;
    [HideInInspector]
    public HurtBox[] activeHurtBoxes;
    [HideInInspector]
    public HitConfirmType hitConfirmType;
    [HideInInspector]
    public float collisionBoxSize;
    [HideInInspector]
    public bool previewInvertRotation;
    [HideInInspector]
    public bool previewMirror;
    [HideInInspector]
    public bool isHitByThisCheck = false;

    private bool currentMirror;

    private Renderer characterRenderer;

    private bool rectangleHitBoxLocationTest;
    private Texture2D rectTexture = new Texture2D(1, 1);

    void Start()
    {
        UpdateRenderer();

        rectangleHitBoxLocationTest = false;
        rectTexture.SetPixel(0, 0, Color.red);
        rectTexture.Apply();
    }

    public Vector3 GetPosition(BodyPart bodyPart)
    {
        foreach (HitBox hitBox in hitBoxes)
        {
            if (bodyPart == hitBox.bodyPart) return hitBox.position.position;
        }
        return Vector3.zero;
    }

    public Transform GetTransform(BodyPart bodyPart)
    {
        foreach (HitBox hitBox in hitBoxes)
        {
            if (bodyPart == hitBox.bodyPart) return hitBox.position;
        }
        return null;
    }

    public Rect GetBounds()
    {
        if (characterRenderer != null)
        {
            return new Rect(characterRenderer.bounds.min.x,
                            characterRenderer.bounds.min.y,
                            characterRenderer.bounds.max.x,
                            characterRenderer.bounds.max.y);
        }
        return new Rect();
    }

    public void UpdateBounds(HurtBox[] hurtBoxes)
    {
        foreach (HurtBox hurtBox in hurtBoxes) if (hurtBox.followXBounds || hurtBox.followYBounds) hurtBox.rendererBounds = GetBounds();
    }

    public void UpdateRenderer()
    {
        bool confirmUpdate = false;
        foreach (HitBox hitBox in hitBoxes)
        {
            if (hitBox.followXBounds || hitBox.followYBounds)
            {
                confirmUpdate = true;
            }
        }

        if (confirmUpdate)
        {
            Renderer[] rendererList = GetComponentsInChildren<Renderer>();
            foreach (Renderer childRenderer in rendererList)
            {
                characterRenderer = childRenderer;
                return;
            }
            Debug.LogWarning("Warning: You are trying to access the character's bounds, but it does not have a renderer.");
        }
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
        // HITBOXES
        if (hitBoxes == null) return;
        int mirrorAdjust = 1;

        foreach (HitBox hitBox in hitBoxes)
        {
            if (hitBox.position == null) continue;
            if (hitBox.hide) continue;
            if (hitBox.State == 1)
            {
                Gizmos.color = Color.red;
            }
            else if (isHit)
            {
                Gizmos.color = Color.magenta;
            }
            else if (hitBox.collisionType == CollisionType.bodyCollider)
            {
                Gizmos.color = Color.yellow;
            }
            else if (hitBox.collisionType == CollisionType.noCollider)
            {
                Gizmos.color = Color.white;
            }
            else if (hitBox.collisionType == CollisionType.throwCollider)
            {
                Gizmos.color = new Color(1f, 0, .5f);
            }
            else
            {
                Gizmos.color = Color.green;
            }

            if (hitBox.shape == HitBoxShape.rectangle && rectangleHitBoxLocationTest)
            {
                Rect hitBoxRectPos = new Rect(hitBox.rect);
                hitBoxRectPos.x *= -mirrorAdjust;
                hitBoxRectPos.width *= -mirrorAdjust;
                hitBoxRectPos.x += hitBox.position.position.x;
                hitBoxRectPos.y += hitBox.position.position.y;
                Gizmos.DrawGUITexture(hitBoxRectPos, rectTexture);
            }

            Vector3 hitBoxPosition = hitBox.position.position + new Vector3(hitBox.offSet.x, hitBox.offSet.y, 0);
            if (FightManager.config == null || !FightManager.config.detect3D_Hits) hitBoxPosition.z = -1;
            if (hitBox.shape == HitBoxShape.circle && hitBox.radius > 0)
            {
                Gizmos.DrawWireSphere(hitBoxPosition, hitBox.radius);
            }
            else if (hitBox.shape == HitBoxShape.rectangle)
            {
                Rect hitBoxRectPosTemp = new Rect(hitBox.rect);
                hitBoxRectPosTemp.x *= -mirrorAdjust;
                hitBoxRectPosTemp.width *= -mirrorAdjust;
                hitBoxRectPosTemp.x += hitBox.position.position.x;
                hitBoxRectPosTemp.y += hitBox.position.position.y;
                Vector3 topLeft = new Vector3(hitBoxRectPosTemp.x, hitBoxRectPosTemp.y);
                Vector3 topRight = new Vector3((hitBoxRectPosTemp.xMax), hitBoxRectPosTemp.y);
                Vector3 bottomLeft = new Vector3(hitBoxRectPosTemp.x, hitBoxRectPosTemp.yMax);
                Vector3 bottomRight = new Vector3((hitBoxRectPosTemp.xMax), hitBoxRectPosTemp.yMax);

                if (hitBox.followXBounds)
                {
                    hitBox.rect.x = 0;
                    topLeft.x = GetBounds().x - (hitBox.rect.width / 2);
                    topRight.x = GetBounds().width + (hitBox.rect.width / 2);
                    bottomLeft.x = GetBounds().x - (hitBox.rect.width / 2);
                    bottomRight.x = GetBounds().width + (hitBox.rect.width / 2);
                }

                if (hitBox.followYBounds)
                {
                    hitBox.rect.y = 0;
                    topLeft.y = GetBounds().height + (hitBox.rect.height / 2);
                    topRight.y = GetBounds().height + (hitBox.rect.height / 2);
                    bottomLeft.y = GetBounds().y - (hitBox.rect.height / 2);
                    bottomRight.y = GetBounds().y - (hitBox.rect.height / 2);
                }

                GizmosDrawRectangle(topLeft, bottomLeft, bottomRight, topRight);
            }

            if (hitBox.collisionType != CollisionType.noCollider)
            {
                if (hitBox.type == HitBoxType.low)
                {
                    Gizmos.color = Color.red;
                }
                else
                {
                    Gizmos.color = Color.yellow;
                }
                Gizmos.DrawWireSphere(hitBoxPosition, .1f);
            }
        }

        // COLLISION BOX SIZE
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collisionBoxSize);


        // HURTBOXES
        if (activeHurtBoxes != null)
        {
            if (hitConfirmType == HitConfirmType.Throw)
            {
                Gizmos.color = new Color(1f, .5f, 0);
            }
            else
            {
                Gizmos.color = Color.cyan;
            }

            foreach (HurtBox hurtBox in activeHurtBoxes)
            {
                if (GetTransform(hurtBox.bodyPart) != null)
                {
                    Vector3 hurtBoxPosition;
                    hurtBoxPosition = GetPosition(hurtBox.bodyPart);
                    if (FightManager.config == null || !FightManager.config.detect3D_Hits) hurtBoxPosition.z = -1;
                    if (hurtBox.shape == HitBoxShape.circle)
                    {
                        float rotY = transform.eulerAngles.y;
                        float atan = Mathf.Atan2(hurtBox.offSet.x, hurtBox.offSet.z);
                        rotY = rotY * Mathf.Deg2Rad + atan;
                        float size = new Vector2(hurtBox.offSet.x, hurtBox.offSet.z).magnitude;
                        float x = size * Mathf.Sin(rotY);
                        float z = size * Mathf.Cos(rotY);
                        hurtBoxPosition.x += x;
                        hurtBoxPosition.z += z;
                        hurtBoxPosition.y += hurtBox.offSet.y;

                        Gizmos.DrawWireSphere(hurtBoxPosition, hurtBox.radius);
                    }
                    else
                    {
                        Vector3 topLeft = new Vector3(hurtBox.rect.x * -mirrorAdjust, hurtBox.rect.y) + hurtBoxPosition;
                        Vector3 topRight = new Vector3((hurtBox.rect.x + hurtBox.rect.width) * -mirrorAdjust, hurtBox.rect.y) + hurtBoxPosition;
                        Vector3 bottomLeft = new Vector3(hurtBox.rect.x * -mirrorAdjust, hurtBox.rect.y + hurtBox.rect.height) + hurtBoxPosition;
                        Vector3 bottomRight = new Vector3((hurtBox.rect.x + hurtBox.rect.width) * -mirrorAdjust, hurtBox.rect.y + hurtBox.rect.height) + hurtBoxPosition;

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
            }
        }
    }
}

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Meta.XR.MRUtilityKit;
using System.Collections;

public class MeshEdgeAligner : MonoBehaviour
{
    //public MeshFilter sourceMesh;
    public GameObject pivot;
    //public MeshFilter targetMesh;
    public GameObject charcter;
    MRUKRoom room;
    GameObject floor;
    public bool isAligned = false;
    Vector3 originpos;
    Quaternion originrot;

    void Start() {
        StartCoroutine(WaitAndAlignMesh());
    }

    IEnumerator WaitAndAlignMesh() {
        // MRUKRoom이 생성될 때까지 대기
        while (GameObject.FindAnyObjectByType<MRUKRoom>() == null) {
            yield return null; // 다음 프레임까지 기다림
        }

        // MRUKRoom이 생기면 AlignMeshes 실행
        room = GameObject.FindAnyObjectByType<MRUKRoom>();
        Debug.Log("found MRUKRoom: " + room.name);

        AlignMeshes(room);  // targetMeshdata 필요 없으면 null 넘겨도 됨
    }

    public void AlignMeshes(MRUKRoom room)
    {
        // GameObject.FindAnyObjectByType<OVRSceneVolumeMeshFilter>().gameObject.GetComponent<MeshRenderer>().enabled = false;
        // room = GameObject.FindAnyObjectByType<MRUKRoom>();
        if (room == null)
        {
            Debug.LogError("MRUKRoom not found in scene.");
            return;
        }

        // RoomMeshAnchor로 찾아서 mesh renderer 끄기
        var anchors = GameObject.FindObjectsOfType<RoomMeshAnchor>();

        foreach (var anchor in anchors){
            var renderer = anchor.GetComponent<MeshRenderer>();
            if(renderer != null){
                renderer.enabled = false;
                Debug.Log("Mesh renderer disabled");
            }
            else{
                Debug.Log("There is no mesh renderer");
            }
        }

        originpos = pivot.transform.position;
        originrot = pivot.transform.rotation;
        floor = room.transform.Find("FLOOR").gameObject;

        Debug.Log("pivot matched===========");
        {
            //pivot.transform.position = floor.transform.position;
            //pivot.transform.rotation = floor.transform.rotation;

            //room.transform.parent = pivot.transform;
            //charcter.transform.parent = pivot.transform;


            //pivot.transform.position = originpos;
            //pivot.transform.rotation = originrot;

            //room.transform.parent = null;   
            //charcter.transform.parent = null;
        } // 기존 방을 움직여서 조정하고 다시 돌려놓는 코드
        {
            // 현재 floor의 월드 변환을 저장
            //Matrix4x4 originalFloorWorldMatrix = floor.transform.localToWorldMatrix;

            //// room과 character의 원래 위치와 회전 저장
            //Vector3 originalRoomPosition = room.transform.position;
            //Quaternion originalRoomRotation = room.transform.rotation;
            //Vector3 originalCharacterPosition = charcter.transform.position;
            //Quaternion originalCharacterRotation = charcter.transform.rotation;

            //// floor를 원점으로 이동시키는 변환 계산
            //Matrix4x4 floorToOriginMatrix = Matrix4x4.TRS(
            //    -floor.transform.position,
            //    Quaternion.Inverse(floor.transform.rotation),
            //    Vector3.one
            //);

            //// room과 character에 변환 적용
            //ApplyTransformation(room.transform, floorToOriginMatrix);
            //ApplyTransformation(charcter.transform, floorToOriginMatrix);

            //// 원래 floor의 월드 변환을 적용하여 원래 위치로 되돌림
            //Matrix4x4 originToOriginalFloorMatrix = originalFloorWorldMatrix;

            //// room과 character에 최종 변환 적용
            //ApplyTransformation(room.transform, originToOriginalFloorMatrix);
            //ApplyTransformation(charcter.transform, originToOriginalFloorMatrix);

        } // 행렬로 변환하는 코드
        {
            GameObject go = Instantiate(floor, floor.transform.position, floor.transform.rotation);
            //pivot.transform.position = floor.transform.position;
            //pivot.transform.rotation = floor.transform.rotation;

            room.transform.parent = go.transform;
            charcter.transform.parent = go.transform;


            go.transform.position = originpos;
            go.transform.rotation = originrot;

            room.transform.parent = null;
            charcter.transform.parent = null;

            Destroy(go);

            

        }
        // 변환을 적용하는 함수
        if (!isAligned) isAligned = true;
        // 변환 구해서 방과 유저에 적용해주기 (방을 같이 안하면 아바타가 혼자 움직임)
    }

    // pivot을 계속 역으로 먹여주면 되지 않을까?

    void ApplyTransformation(Transform transform, Matrix4x4 matrix)
    {
        // ��ġ ����
        transform.position = matrix.MultiplyPoint3x4(transform.position);

        // ȸ�� ����
        transform.rotation = matrix.rotation * transform.rotation;

        // ����: �������� �������� ����
    }
}
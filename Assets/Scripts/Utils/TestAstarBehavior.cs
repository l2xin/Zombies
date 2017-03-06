using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks.Movement.AstarPathfindingProject;
using BehaviorDesigner.Runtime.Tasks.Movement.AstarPathfindingProject.RichAI;

public class TestAstarBehavior : MonoBehaviour
{
    public GameObject mainBot;
    public GameObject followGroup;
    public GameObject[] skeletons;
    private BehaviorTree behaviorTree;

    private enum BehaviorSelectionType { LeaderFollow, Last }

    private BehaviorSelectionType selectionType = BehaviorSelectionType.LeaderFollow;
    private BehaviorSelectionType prevSelectionType = BehaviorSelectionType.LeaderFollow;
    private LeaderFollow leaderFollow;

    public void Start()
    {
        Debug.Log("TestAstarBehavior Start...");
        mainBot = GameObject.Find("Warrior");
        behaviorTree = Camera.main.GetComponent<BehaviorTree>();
        GameObject skeletonsParent = GameObject.Find("Skeletons").gameObject;
        skeletons = new GameObject[skeletonsParent.transform.childCount];
        for (int i = 0; i < skeletonsParent.transform.childCount; i++)
        {
            GameObject skeleton = skeletonsParent.transform.GetChild(i).gameObject;
            skeletons[i] = skeleton;
        }
        //Leader Follow
        //leaderFollow = behaviorTree.FindTask<LeaderFollow>();

        leaderFollow = behaviorTree.FindTaskWithName("Leader Follow") as LeaderFollow;

        leaderFollow.leader = mainBot;

        leaderFollow = behaviorTree.FindTask<LeaderFollow>();
        leaderFollow.agents = new SharedGameObject[skeletons.Length];
        for (int i = 0; i < skeletons.Length; i++)
        {
            leaderFollow.agents[i] = skeletons[i];
        }

        SelectionChanged();
    }

    private void SelectionChanged()
    {
        DisableAll();
        mainBot.transform.position = new Vector3(0, 2, -130);
        mainBot.SetActive(true);

        StartCoroutine("EnableBehavior");
    }

    private void DisableAll()
    {
        StopCoroutine("EnableBehavior");

        behaviorTree.DisableBehavior();

        mainBot.SetActive(false);
        Camera.main.transform.position = new Vector3(0, 90, 0);
    }

    private IEnumerator EnableBehavior()
    {
        yield return new WaitForSeconds(0.5f);

        behaviorTree.EnableBehavior();
    }
}

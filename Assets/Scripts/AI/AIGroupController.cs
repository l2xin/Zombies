using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks.Movement.AstarPathfindingProject;
using BehaviorDesigner.Runtime.Tasks.Movement.AstarPathfindingProject.AIPath;

public class AIGroupController : MonoBehaviour
{

    private Dictionary<ulong, BehaviorTree> behaviorTreeGroup;
    private int groupIndex = 0;
    private int currentIndex = 0;
    private List<GameObject> agentList;
    private BehaviorTree leaderFollowBehaviorTree;
    private LeaderFollow leaderFollow;
    private Player myPlayer;

    private void Awake()
    {
        myPlayer = gameObject.GetComponent<Player>();
        groupIndex = 0;
        agentList = new List<GameObject>();
        behaviorTreeGroup = new Dictionary<ulong, BehaviorTree>();
        FightManager.OnEnemyAddHandler += FightManager_OnEnemyAddHandler;

        if (behaviorTreeGroup.ContainsKey(myPlayer.id) == false)
        {
            BehaviorTree behaviorTree = Camera.main.GetComponent<BehaviorTree>();
            behaviorTreeGroup.Add(myPlayer.id, behaviorTree);
        }
        if (behaviorTreeGroup.ContainsKey(myPlayer.id))
        {
            BehaviorTree behaviorTree = behaviorTreeGroup[myPlayer.id];
            behaviorTree.Group = groupIndex;
            leaderFollowBehaviorTree = behaviorTree;
            leaderFollow = behaviorTree.FindTaskWithName("Leader Follow") as LeaderFollow;
            leaderFollow.leader = gameObject;
        }
    }

    private void FightManager_OnEnemyAddHandler(GameObject go)
    {
        AddGroupBehaviorTree(FightManager.hero.id, FightManager.hero.gameObject, go);
        currentIndex++;
    }

    public void AddGroupBehaviorTree(ulong id, GameObject leader, GameObject skeleton)
    {
        leaderFollowBehaviorTree.DisableBehavior();
        agentList.Add(skeleton);
        SharedGameObject[] agents = new SharedGameObject[agentList.Count];
        for (int i = 0; i < agentList.Count; i++)
        {
            agents[i] = agentList[i];
        }
        leaderFollow.agents = agents;
        leaderFollowBehaviorTree.EnableBehavior();
    }

    void OnDestroy()
    {
        FightManager.OnEnemyAddHandler -= FightManager_OnEnemyAddHandler;
    }
}

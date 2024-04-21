using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BansheeGz.BGSpline.Components;
using BansheeGz.BGSpline.Curve;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using System.Diagnostics;



public struct ActiveBallList
{
    public List<GameObject> ballList;
    public bool isMoving;
    public bool isInTransition;
}

public enum BallColor
{
    red,
    green,
    blue
}

public class MoveBalls : MonoBehaviour
{
    [Header("UI")]
    public Slider pathSpeedSlider;
    public TMP_InputField ballCountInputField;
    public ResultScript playResult;

    [Header("Ball colors")]
    public GameObject redBall;
    public GameObject greenBall;
    public GameObject blueBall;
    public GameObject yellowBall;

    [Header("Ball settings")]
    public float pathSpeed;
    public float mergeSpeed;
    public int ballCount;
    public GameObject particlePrefab;
    public AudioSource audioSource;

    public Ease easeType;
    public Ease mergeEaseType;

    // Private
    private Stopwatch stopwatch = new Stopwatch();
    private List<GameObject> ballList;
    private GameObject ballsContainerGO;
    private GameObject removedBallsContainer;
    private GameManager gameManager;

    private BGCurve bgCurve;
    private float distance = 0;
    private float tempSpeed = 0;

    private int headballIndex;
    private SectionData sectionData;
    [SerializeField]
    private int addBallIndex;
    private int touchedBallIndex;
    private float ballRadius;
    private bool gameEnded = false;

    private BGCcMath bgCcMathComponent;
    private Renderer redBallRenderer;
    private Renderer greenBallRenderer;
    private Renderer blueBallRenderer;

    private void Start()
    {
        ballRadius = redBall.transform.localScale.x * 0.8f;
        headballIndex = 0;
        addBallIndex = -1;
        DOTween.SetTweensCapacity(10000000, 50);
        pathSpeed = pathSpeedSlider.value;
        ballCount = int.Parse(ballCountInputField.text);

        tempSpeed = pathSpeed;

        bgCurve = GetComponent<BGCurve>();
        ballList = new List<GameObject>();
        gameManager = FindObjectOfType<GameManager>();

        ballsContainerGO = new GameObject();
        ballsContainerGO.name = "Balls Container";

        removedBallsContainer = new GameObject();
        removedBallsContainer.name = "Removed Balls Container";

        bgCcMathComponent = GetComponent<BGCcMath>();
        redBallRenderer = redBall.GetComponent<Renderer>();
        greenBallRenderer = greenBall.GetComponent<Renderer>();
        blueBallRenderer = blueBall.GetComponent<Renderer>();

        for (int i = 0; i < ballCount; i++)
            CreateNewBall();

        sectionData = new SectionData();
        stopwatch.Start();
    }

    private void Update()
    {
        if (!gameEnded)
        {
            if (addBallIndex != -1 && sectionData.ballSections.Count > 1 && addBallIndex < headballIndex)
                MoveStoppedBallsAlongPath();

            if (ballList.Count > 0)
            {
                if (stopwatch.ElapsedMilliseconds < 1500f)
                    pathSpeed = 20f;
                else
                {
                    pathSpeed = tempSpeed;
                    stopwatch.Stop();
                }
                MoveAllBallsAlongPath();
            }

            if (headballIndex != 0)
                JoinStoppedSections(headballIndex, headballIndex - 1);

            if (addBallIndex != -1)
                AddNewBallAndDeleteMatched();

            if (CheckIfActiveEndsMatch())
                MergeActiveEnds();

            if (ballList.Count == 0)
            {
                gameEnded = true;
                playResult.Won();
            }

            if (IsHeadballAtEnd())
            {
                gameEnded = true;
                foreach (GameObject ball in ballList)
                {
                    Instantiate(particlePrefab, ball.transform.position, Quaternion.identity);
                }
                foreach (GameObject ball in ballList)
                {
                    Destroy(ball);
                }
                ballList.Clear();
                playResult.Lose();
            }

        }
    }

    private bool IsHeadballAtEnd()
    {
        float endDistance;
        bgCcMathComponent.CalcPositionByClosestPoint(bgCurve[bgCurve.PointsCount - 1].PositionWorld, out endDistance);
        return distance >= endDistance;
    }

    public void AddNewBallAt(GameObject go, int index, int touchedBallIdx)
    {
        addBallIndex = index;
        touchedBallIndex = touchedBallIdx;

        if (index > ballList.Count)
            ballList.Add(go);
        else
            ballList.Insert(index, go);

        go.transform.parent = ballsContainerGO.transform;
        go.transform.SetSiblingIndex(index);

        if (touchedBallIdx < headballIndex)
            headballIndex++;

        sectionData.OnAddModifySections(touchedBallIdx);

        PushSectionForwardByUnit();
        if (addBallIndex != -1)
            AddNewBallAndDeleteMatched();
    }


    public static BallColor GetRandomBallColor()
    {
        int rInt = Random.Range(0, 3);
        return (BallColor)rInt;
    }


    private void CreateNewBall()
    {
        switch (GetRandomBallColor())
        {
            case BallColor.red:
                InstantiateBall(redBall);
                break;

            case BallColor.green:
                InstantiateBall(greenBall);
                break;

            case BallColor.blue:
                InstantiateBall(blueBall);
                break;
        }
    }

    private void InstantiateBall(GameObject ballGameObject)
    {
        GameObject go = Instantiate(ballGameObject, bgCurve[0].PositionWorld, Quaternion.identity, ballsContainerGO.transform);
        go.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
        go.SetActive(false);
        ballList.Add(go.gameObject);
    }

    private void MoveStoppedBallsAlongPath()
    {
        int sectionKey = sectionData.GetSectionKey(addBallIndex);
        int sectionKeyVal = sectionData.ballSections[sectionKey];
        int movingBallCount = 1;

        float sectionHeadDist;
        bgCcMathComponent.CalcPositionByClosestPoint(ballList[sectionKeyVal].transform.position, out sectionHeadDist);

        Vector3 tangent;
        Vector3 trailPos;
        float currentBallDist;
        for (int i = sectionKeyVal + 1; i <= sectionKey; i++)
        {
            currentBallDist = sectionHeadDist - movingBallCount * ballRadius / 1.2f;
            trailPos = bgCcMathComponent.CalcPositionAndTangentByDistance(currentBallDist, out tangent);

            if (i == addBallIndex && addBallIndex != -1)
                ballList[i].transform.DOMove(trailPos, 0.5f)
                    .SetEase(easeType);
            else
                ballList[i].transform.DOMove(trailPos, 1);

            movingBallCount++;
        }
    }

    private void MoveAllBallsAlongPath()
    {
        Vector3 tangent;
        int movingBallCount = 1;

        distance += pathSpeed * Time.deltaTime;

        Vector3 headPos = bgCcMathComponent.CalcPositionAndTangentByDistance(distance, out tangent);
        ballList[headballIndex].transform.DOMove(headPos, 1);
        ballList[headballIndex].transform.rotation = Quaternion.LookRotation(tangent);

        if (!ballList[headballIndex].activeSelf)
            ballList[headballIndex].SetActive(true);

        for (int i = headballIndex + 1; i < ballList.Count; i++)
        {
            float currentBallDist = distance - movingBallCount * ballRadius;
            Vector3 trailPos = bgCcMathComponent.CalcPositionAndTangentByDistance(currentBallDist, out tangent);

            if (i == addBallIndex && addBallIndex != -1)
                ballList[i].transform.DOMove(trailPos, 0.5f)
                    .SetEase(easeType);
            else
                ballList[i].transform.DOMove(trailPos, 1);

            ballList[i].transform.rotation = Quaternion.LookRotation(tangent);

            if (!ballList[i].activeSelf)
                ballList[i].SetActive(true);

            movingBallCount++;
        }
    }


    private void PushSectionForwardByUnit()
    {
        int sectionKey = sectionData.GetSectionKey(addBallIndex);
        int sectionKeyVal = sectionData.ballSections[sectionKey];

        float modifiedDistance;
        Vector3 tangent;

        bgCcMathComponent.CalcPositionByClosestPoint(ballList[sectionKeyVal].transform.position, out modifiedDistance);
        modifiedDistance += ballRadius;

        if (addBallIndex >= headballIndex)
            distance = modifiedDistance;

        Vector3 movedPos = bgCcMathComponent.CalcPositionAndTangentByDistance(modifiedDistance, out tangent);
        ballList[sectionKeyVal].transform.DOMove(movedPos, 1);
        ballList[sectionKeyVal].transform.rotation = Quaternion.LookRotation(tangent);
    }

    private void JoinStoppedSections(int currentIdx, int nextSectionIdx)
    {
        float nextSecdist;
        bgCcMathComponent.CalcPositionByClosestPoint(ballList[nextSectionIdx].transform.position, out nextSecdist);

        if (nextSecdist - distance <= ballRadius)
        {
            int nextSectionKeyVal;
            sectionData.ballSections.TryGetValue(nextSectionIdx, out nextSectionKeyVal);
            headballIndex = nextSectionKeyVal;

            MergeSections(currentIdx, nextSectionKeyVal);
            RemoveMatchedBalls(nextSectionIdx, ballList[nextSectionIdx]);

            if (ballList.Count > 0)
            {
                bgCcMathComponent.CalcPositionByClosestPoint(ballList[headballIndex].transform.position, out nextSecdist);
                distance = nextSecdist;
            }
        }
    }

    private void MergeSections(int currentIdx, int nextSectionKeyVal)
    {
        sectionData.ballSections.Remove(currentIdx - 1);
        sectionData.ballSections[int.MaxValue] = nextSectionKeyVal;
    }


    private void AddNewBallAndDeleteMatched()
    {
        int sectionKey = sectionData.GetSectionKey(addBallIndex);
        int sectionKeyVal = sectionData.ballSections[sectionKey];

        float neighbourDist = 0;
        Vector3 currentTangent;
        Vector3 actualPos = Vector3.zero;
        Vector3 neighbourPos = Vector3.zero;

        int end = sectionKey == int.MaxValue ? ballList.Count - 1 : sectionKey;
        if (addBallIndex == end)
        {
            neighbourPos = ballList[addBallIndex - 1].transform.position;
            bgCcMathComponent.CalcPositionByClosestPoint(neighbourPos, out neighbourDist);
            actualPos = bgCcMathComponent.CalcPositionAndTangentByDistance(neighbourDist - ballRadius, out currentTangent);
        }
        else
        {
            neighbourPos = ballList[addBallIndex + 1].transform.position;
            bgCcMathComponent.CalcPositionByClosestPoint(neighbourPos, out neighbourDist);
            actualPos = bgCcMathComponent.CalcPositionAndTangentByDistance(neighbourDist + ballRadius, out currentTangent);
        }

        Vector2 currentPos = new Vector2(ballList[addBallIndex].transform.position.x, ballList[addBallIndex].transform.position.y);
        float isNear = Vector2.Distance(actualPos, currentPos);

        if (isNear <= 0.5f)
        {
            RemoveMatchedBalls(addBallIndex, ballList[addBallIndex]);
            addBallIndex = -1;
        }
    }

    private void RemoveMatchedBalls(int index, GameObject go)
    {
        int front = index;
        int back = index;

        Color ballColor = go.GetComponent<Renderer>().material.GetColor("_Color");

        int sectionKey = sectionData.GetSectionKey(index);
        int sectionKeyVal;
        sectionData.ballSections.TryGetValue(sectionKey, out sectionKeyVal);

        for (int i = index - 1; i >= sectionKeyVal; i--)
        {
            Color currrentBallColor = ballList[i].GetComponent<Renderer>().material.GetColor("_Color");
            if (ballColor == currrentBallColor)
                front = i;
            else
                break;
        }

        int end = sectionKey == int.MaxValue ? ballList.Count - 1 : sectionKey;
        for (int i = index + 1; i <= end; i++)
        {
            Color currrentBallColor = ballList[i].GetComponent<Renderer>().material.GetColor("_Color");
            if (ballColor == currrentBallColor)
                back = i;
            else
                break;
        }

        if (back - front >= 2)
        {
            if (back > headballIndex)
            {
                if (front == sectionKeyVal && back == ballList.Count - 1)
                {
                    if (sectionData.ballSections.Count > 1)
                    {
                        int nextSectionFront;
                        sectionData.ballSections.TryGetValue(front - 1, out nextSectionFront);
                        headballIndex = nextSectionFront;
                    }
                }
                else
                {
                    if (front >= sectionKeyVal && back != ballList.Count - 1)
                    {
                        headballIndex = front;
                    }
                }
            }
            else
            {
                headballIndex -= (back - front + 1);
            }
            RemoveBalls(front, back - front + 1);

            if (back > headballIndex && ballList.Count > 0)
                bgCcMathComponent.CalcPositionByClosestPoint(ballList[headballIndex].transform.position, out distance);
        }
    }

    private void RemoveBalls(int atIndex, int range)
    {
        for (int i = 0; i < range; i++)
        {
            CreateParticleAndDestroy(ballList[atIndex + i]);

            ballList[atIndex + i].transform.parent = removedBallsContainer.transform;
            ballList[atIndex + i].SetActive(false);
            gameManager.BallDestroyed(i);

        }
        audioSource.Play();
        ballList.RemoveRange(atIndex, range);

        OnDeleteModifySections(atIndex, range);
    }

    private void CreateParticleAndDestroy(GameObject obj)
    {
        GameObject particle = Instantiate(particlePrefab, obj.transform.position, Quaternion.identity);
        Destroy(particle, 1f); 
    }


    private void OnDeleteModifySections(int atIndex, int range)
    {
        int sectionKey = sectionData.GetSectionKey(atIndex);

        int sectionKeyVal;
        sectionData.ballSections.TryGetValue(sectionKey, out sectionKeyVal);

        if (atIndex == sectionKeyVal && atIndex + range == ballList.Count + range)
        {
            sectionData.DeleteEntireSection(atIndex, range, sectionKey, ballList.Count);
        }
        else
        {
            sectionData.DeletePartialSection(atIndex, range, sectionKey, sectionKeyVal, ballList.Count);
        }
    }

    private bool CheckIfActiveEndsMatch()
    {
        if (sectionData.ballSections.Count <= 1)
            return false;

        Color headBallColor = ballList[headballIndex].GetComponent<Renderer>().material.GetColor("_Color");
        Color nextSectionEndColor = ballList[headballIndex - 1].GetComponent<Renderer>().material.GetColor("_Color");

        if (headBallColor == nextSectionEndColor)
            return true;

        return false;
    }

    private void MergeActiveEnds()
    {
        int sectionKey = headballIndex - 1;
        int sectionKeyVal = sectionData.ballSections[sectionKey];

        int movingBallCount = 1;
        float sectionHeadDist;
        bgCcMathComponent.CalcPositionByClosestPoint(ballList[sectionKey].transform.position, out sectionHeadDist);
        sectionHeadDist -= mergeSpeed * Time.deltaTime;

        Vector3 tangent;
        Vector3 trailPos = bgCcMathComponent.CalcPositionAndTangentByDistance(sectionHeadDist, out tangent);
        ballList[sectionKey].transform.DOMove(trailPos, 0.3f)
            .SetEase(mergeEaseType);

        for (int i = sectionKey - 1; i >= sectionKeyVal; i--)
        {
            float currentBallDist = sectionHeadDist + movingBallCount * ballRadius;
            trailPos = bgCcMathComponent.CalcPositionAndTangentByDistance(currentBallDist, out tangent);

            ballList[i].transform.DOMove(trailPos, 0.3f)
                .SetEase(easeType);
            ballList[i].transform.rotation = Quaternion.LookRotation(tangent);

            movingBallCount++;
        }
    }

    public void RestartGame()
    {
        distance = 0;
        tempSpeed = pathSpeedSlider.value;
        headballIndex = 0;
        addBallIndex = -1;
        gameEnded = false;

        foreach (GameObject ball in ballList)
        {
            Destroy(ball);
        }
        ballList.Clear();
        for (int i = 0; i < ballCount; i++)
        {
            CreateNewBall();
        }

        pathSpeed = pathSpeedSlider.value;

        gameEnded = false;
        Time.timeScale = 1f;
        stopwatch.Restart();
    }
}

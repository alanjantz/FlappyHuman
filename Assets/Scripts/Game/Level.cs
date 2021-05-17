using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Level : MonoBehaviour
{
    private static Level _instance;

    private const float CAMERA_SIZE = 50f;
    private const float CAMERA_LEFT_EDGE = -100f;
    private const float CAMERA_RIGHT_EDGE = 100f;
    private const float GROUND_DESTROY_X_POSITION = -200f;
    private const float CLOUD_DESTROY_X_POSITION = -160f;
    private const float CLOUD_SPAWN_X_POSITION = +160f;
    private const float CLOUD_SPAWN_Y_POSITION = +30f;
    private const float PERSON_POSITION = 0f;

    private List<Transform> grounds;
    private List<Transform> clouds;
    private List<Pipe> pipes;
    private int pipesSpawned;
    private float pipeSpawnTimer;
    private float pipeSpawnTimerMax;
    private float moveSpeed;
    private float gap;
    private float cloudSpawnTimer;
    private int points;
    private GameState state;

    private void Awake()
    {
        _instance = this;
        SpawnInitialGround();
        SpawnInitialClouds();
        Restart(GameState.Starting);
    }

    public void Restart(GameState gameState = GameState.Waiting)
    {
        foreach (var pipe in pipes ?? Enumerable.Empty<Pipe>())
            pipe.DestroySelf();

        GameOverWindow.GetInstance()?.Hide();
        state = gameState;
        pipes = new List<Pipe>();
        SetDifficulty(Difficulty.Easy);
        pipesSpawned = points = 0;
    }

    private void Update()
    {
        SetOnDiedEvent();
        bool havePlayers = BodySourceView.GetInstance().GetBodies().Any();

        switch (state)
        {
            case GameState.Starting:
                GameOverWindow.GetInstance()?.Show();
                break;
            case GameState.Waiting:
                if (havePlayers)
                    state = GameState.Playing;
                break;
            case GameState.Playing:
                HandlePipeMovement();
                HandlePipeSpawning();
                HandleGround();
                HandleClouds();

                if (!havePlayers)
                    state = GameState.Waiting;
                break;
            default:
                break;
        }
    }

    public static Level GetInstance() => _instance;

    public int GetPoints() => points;

    public GameState GetState() => state;

    public static bool IsInitializingOrDead => new GameState[] { GameState.Starting, GameState.Dead }.Contains(_instance?.GetState() ?? GameState.Starting);

    public static bool IsInitializing => (_instance?.GetState() ?? GameState.Starting) == GameState.Starting;

    public static bool IsDead => (_instance?.GetState() ?? GameState.Starting) == GameState.Dead;

    private void SetOnDiedEvent()
    {
        var bodyJoints = BodyJoint.GetJoints();

        if (bodyJoints != null)
            foreach (var joint in bodyJoints.Where(x => !x.GetHasOnDiedEvent()))
            {
                joint.OnDied += OnDied;
                joint.SetHasOnDiedEvent();
            }
    }

    private void HandlePipeMovement()
    {
        for (int i = 0; i < pipes.Count; i++)
        {
            var pipe = pipes[i];

            bool isToTheRight = pipe.XPosition > PERSON_POSITION;

            pipe.Move(moveSpeed);

            if (isToTheRight && pipe.XPosition <= PERSON_POSITION && pipe.IsBottom)
            {
                points++;
                SoundManager.PlaySound(Sounds.Score);
            }

            if (pipe.XPosition < CAMERA_LEFT_EDGE)
            {
                pipe.DestroySelf();
                pipes.Remove(pipe);
                i--;
            }
        }
    }

    private void HandlePipeSpawning()
    {
        float getHeight()
        {
            float heightEdgeLimit = 30f;
            float minHeight = gap * .5f + heightEdgeLimit;
            float totalHeight = CAMERA_SIZE * 2f;
            float maxHeight = totalHeight - gap * .5f - heightEdgeLimit;

            return UnityEngine.Random.Range(minHeight, maxHeight);
        }

        pipeSpawnTimer -= Time.deltaTime;

        if (pipeSpawnTimer < 0)
        {
            pipeSpawnTimer += pipeSpawnTimerMax;

            float height = getHeight();
            CreateGapPipes(height, gap, CAMERA_RIGHT_EDGE);
        }
    }

    private void CreateGapPipes(float gapY, float gapSize, float xPosition)
    {
        CreatePipe(gapY - gapSize * .5f, xPosition);
        CreatePipe(CAMERA_SIZE * 2f - gapY - gapSize * .5f, xPosition, false);
        pipesSpawned++;
        SetDifficulty(GetDifficulty());
    }

    private void CreatePipe(float height, float xPosition, bool createOnBottom = true)
        => pipes.Add(new Pipe(height, xPosition, createOnBottom));

    private void SpawnInitialGround()
    {
        grounds = new List<Transform>();
        Transform groundTransform;
        float groundY = -47.5f;
        float groundWidth = 192f;
        groundTransform = Instantiate(GameAssets.GetInstance().Ground, new Vector3(0, groundY, 0), Quaternion.identity);
        grounds.Add(groundTransform);
        groundTransform = Instantiate(GameAssets.GetInstance().Ground, new Vector3(groundWidth, groundY, 0), Quaternion.identity);
        grounds.Add(groundTransform);
        groundTransform = Instantiate(GameAssets.GetInstance().Ground, new Vector3(groundWidth * 2f, groundY, 0), Quaternion.identity);
        grounds.Add(groundTransform);
    }

    private void HandleGround()
    {

        foreach (var ground in grounds)
        {
            ground.position += new Vector3(-1, 0, 0) * moveSpeed * Time.deltaTime;

            if (ground.position.x < GROUND_DESTROY_X_POSITION)
            {
                float rightMostXPosition = -100f;
                for (int i = 0; i < grounds.Count; i++)
                    if (grounds[i].position.x > rightMostXPosition)
                        rightMostXPosition = grounds[i].position.x;

                float groundWidth = 192f;
                ground.position = new Vector3(rightMostXPosition + groundWidth, ground.position.y, ground.position.z);
            }
        }
    }

    private void SpawnInitialClouds()
    {
        clouds = new List<Transform>();
        Transform cloudTransform;
        cloudTransform = Instantiate(GetCloudPrefabTransform(), new Vector3(0, CLOUD_SPAWN_Y_POSITION, 0), Quaternion.identity);
        clouds.Add(cloudTransform);
    }

    private Transform GetCloudPrefabTransform()
    {
        switch (Random.Range(0, 3))
        {
            default:
            case 0: return GameAssets.GetInstance().Clouds1;
            case 1: return GameAssets.GetInstance().Clouds2;
            case 2: return GameAssets.GetInstance().Clouds3;
        }
    }

    private void HandleClouds()
    {
        cloudSpawnTimer -= Time.deltaTime;
        if (cloudSpawnTimer < 0)
        {
            float cloudSpawnTimerMax = 6f;
            cloudSpawnTimer = cloudSpawnTimerMax;
            Transform cloudTransform = Instantiate(GetCloudPrefabTransform(), new Vector3(CLOUD_SPAWN_X_POSITION, CLOUD_SPAWN_Y_POSITION, 0), Quaternion.identity);
            clouds.Add(cloudTransform);
        }

        for (int i = 0; i < clouds.Count; i++)
        {
            Transform cloudTransform = clouds[i];
            cloudTransform.position += new Vector3(-1, 0, 0) * moveSpeed * Time.deltaTime * .7f;

            if (cloudTransform.position.x < CLOUD_DESTROY_X_POSITION)
            {
                Destroy(cloudTransform.gameObject);
                clouds.RemoveAt(i);
                i--;
            }
        }
    }

    private void SetDifficulty(Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.Impossible:
                gap = 15f;
                pipeSpawnTimerMax = 2f;
                moveSpeed = 40f;
                break;
            case Difficulty.Hard:
                gap = 20f;
                pipeSpawnTimerMax = 1.85f;
                moveSpeed = 35f;
                break;
            case Difficulty.Medium:
                gap = 25f;
                pipeSpawnTimerMax = 1.65f;
                moveSpeed = 30f;
                break;
            case Difficulty.Easy:
            default:
                gap = 30f;
                pipeSpawnTimerMax = 1.5f;
                moveSpeed = 25f;
                break;
        }
    }

    private Difficulty GetDifficulty()
    {
        if (pipesSpawned >= 20) return Difficulty.Impossible;
        if (pipesSpawned >= 10) return Difficulty.Hard;
        if (pipesSpawned >= 5) return Difficulty.Medium;
        return Difficulty.Easy;
    }

    private void OnDied(object sender, System.EventArgs e)
    {
        if (state != GameState.Dead)
            EndGame();

        state = GameState.Dead;
    }

    private void EndGame()
    {
        var newHighscore = Score.TrySetNewHighScore(points);
        GameOverWindow.GetInstance().Show(newHighscore);
    }

    private class Pipe
    {
        private const float PIPE_BODY_WIDTH = 7.8f;
        private const float PIPE_HEAD_HEIGHT = 3.75f;

        private Transform _head;
        private Transform _body;
        public bool IsBottom { get; private set; }

        public Pipe(float height, float xPosition, bool createOnBottom)
        {
            CreateHead(height, xPosition, createOnBottom);
            CreateBody(height, xPosition, createOnBottom);
            IsBottom = createOnBottom;
        }

        private void CreateHead(float height, float xPosition, bool createOnBottom)
        {
            _head = Instantiate(GameAssets.GetInstance().PipeHead);
            float pipeHeadYPosition;
            if (createOnBottom)
                pipeHeadYPosition = -CAMERA_SIZE + height - PIPE_HEAD_HEIGHT * .48f;
            else
                pipeHeadYPosition = CAMERA_SIZE - height + PIPE_HEAD_HEIGHT * .48f;
            _head.position = new Vector3(xPosition, pipeHeadYPosition);
        }

        private void CreateBody(float height, float xPosition, bool createOnBottom)
        {
            _body = Instantiate(GameAssets.GetInstance().PipeBody);
            float pipeBodyYPosition;
            if (createOnBottom)
                pipeBodyYPosition = -CAMERA_SIZE;
            else
            {
                pipeBodyYPosition = CAMERA_SIZE;
                _body.localScale = new Vector3(1, -1, 1);
            }
            _body.position = new Vector3(xPosition, pipeBodyYPosition);

            SpriteRenderer pipeBodySpriteRenderer = _body.GetComponent<SpriteRenderer>();
            pipeBodySpriteRenderer.size = new Vector2(PIPE_BODY_WIDTH, height);

            BoxCollider2D pipeBodyCollider = _body.GetComponent<BoxCollider2D>();
            pipeBodyCollider.size = new Vector2(PIPE_BODY_WIDTH, height);
            pipeBodyCollider.offset = new Vector2(0f, height * .5f);
        }

        public void Move(float moveSpeed)
        {
            _head.position += new Vector3(-1, 0, 0) * moveSpeed * Time.deltaTime;
            _body.position += new Vector3(-1, 0, 0) * moveSpeed * Time.deltaTime;
        }

        public float XPosition => _head.position.x;

        public void DestroySelf()
        {
            Destroy(_head.gameObject);
            Destroy(_body.gameObject);
        }
    }
}

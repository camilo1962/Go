using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Color = UnityEngine.Color;
using TMPro;

// Reglas de Tromp-Taylor a través de la biblioteca de Sensei
// El Go se juega en una cuadrícula de puntos de 19x19, por dos jugadores llamados Blanco y Negro.
// la cuadrícula cuadrada de 19 x 19 se construye mediante una matriz bidimensional de objetos del juego
// cada objeto del juego contiene una clase de Punto, que realiza un seguimiento de la piedra del punto, las piedras adyacentes (vecinas) y las piedras conectadas (grupo)
// Cada punto de la cuadrícula puede estar coloreado en negro, blanco o vacío.
// el color de la piedra que ocupa cada punto se registra en la clase de puntos
// Se dice que un punto P, no coloreado C, llega a C, si hay un camino de puntos adyacentes (vertical u horizontalmente) del color de P desde P hasta un punto de color C.
// la función getGroup implementa un algoritmo de búsqueda en amplitud para encontrar todas las piedras que están conectadas vertical u horizontalmente a cualquier piedra determinada
// Limpiar un color es el proceso de vaciar todos los puntos de ese color que no llegan al vacío.
// las funciones getReach y validGroup calculan juntas el 'alcance' de un grupo de piedras, que contiene todas las libertades del grupo
// Comenzando con una cuadrícula vacía, los jugadores alternan turnos, comenzando con las negras.
// la función Actualizar alterna entre movimientos en blanco y negro
// Un turno es un pase; o un movimiento que no repite un color de cuadrícula anterior.[1]
// hacer
// Un movimiento consiste en colorear un punto vacío del propio color; luego borrar el color del oponente y luego borrar el propio color.
// la función playStone asigna una piedra del color del jugador a uno de los puntos de la matriz; el chequeVecinos finctionm
// El juego termina después de dos pases consecutivos.
// hacer
// La puntuación de un jugador es la cantidad de puntos de su color, más la cantidad de puntos vacíos que alcanzan solo su color.
// hacer
// El jugador con mayor puntuación al final del juego es el ganador. Las puntuaciones iguales resultan en un empate.


public class goban13 : MonoBehaviour
{
    private Sonido sonido;
    private const int BOARD_SIZE = 13;

    private Stone13[,] stones = new Stone13[BOARD_SIZE,BOARD_SIZE];
    public GameObject[,] points;

    public GameObject blancaGanada;
    public GameObject negraGanada;
    public float distanciaEntreFichas = 2f; // Distancia entre los prefabs
    public Vector3 zonaAleatoriaBlancas = new Vector3(-10f, 0.8f, 3f); // Rango del área aleatoria en XYZ
    public Vector3 zonaAleatoriaNegras = new Vector3(17f, 0.8f, 2.5f); // Rango del área aleatoria en XYZ

    public Camera currentCamera;
    public Material[] colors;
    public GameObject go_stone;

    private string empty = "empty";
    private string black = "black";
    private string white = "white";
    private bool turnBlack = true;

    private int negras;
    private int blancas;
    private int ganablancas;
    private int gananegras;

    public TMP_Text negrasText;
    public TMP_Text blancasText;
    public TMP_Text ganaBlancasText;
    public TMP_Text ganaNegrasText;
    public TMP_Text totalBlancasText;
    public TMP_Text totalNegrasText;
    public TMP_Text finalNegrasText;
    public TMP_Text finalBlancasText;
    public TMP_Text ganador;

    public GameObject panelGameOver;

    private void Awake()
    {
        generateAllPoints(BOARD_SIZE);
        //playStone(black,9,9);
    }

    private void Start()
    {
        panelGameOver.SetActive(false);

    }

    private void Update()
    {
        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out info))
        {
            Vector2Int hitposition = getPointIndex(info.transform.gameObject);

            if (Input.GetMouseButtonDown(0) && points[hitposition.x,hitposition.y].tag == empty)
                if (turnBlack)
                {
                stones[hitposition.x,hitposition.y] = playStone(black,hitposition.x,hitposition.y);
                    Sonido.instance.PlayVoiceChessDown();
                    turnBlack = false;
                }
                else
                {
                stones[hitposition.x,hitposition.y] = playStone(white,hitposition.x,hitposition.y);
                    Sonido.instance.PlayVoiceChessDown();
                    turnBlack = true;
                }
                
        }
    }

    public void Paso()
    {
        if (turnBlack == true)
        {
            turnBlack = false;

        }
        else
        {
            turnBlack = true;
        }
    }

    private GameObject generateOnePoint(int x, int y)
    {
        GameObject point = new GameObject(string.Format("points[{0},{1}]",x,y),typeof(BoxCollider));
        point.AddComponent<Point13>();
        Vector3 temp = new Vector3(x,0,y);
        point.transform.position += temp;
        point.tag=empty;

        return point;
    }

    private void generateAllPoints(int boardSize)
    {
        points = new GameObject[boardSize,boardSize];
        for (int x=0;x<boardSize;x++)
            for (int y=0;y<boardSize;y++)
                points[x,y] = generateOnePoint(x,y);
    }

    private Vector2Int getPointIndex(GameObject hitInfo)
    {
        for (int x=0; x< BOARD_SIZE; x++)
            for (int y=0; y< BOARD_SIZE; y++)
                if (points[x,y] == hitInfo)
                    return new Vector2Int(x,y);
        return -Vector2Int.one;
    }

    private Stone13 playStone(string color, int x, int y)
    {        
        Stone13 stone = Instantiate(go_stone).GetComponent<Stone13>();

        stone.color=color;
        stone.x=x;
        stone.y=y;

        int team;
        if(stone.color==black)
        {
            team=0;
            negras++; // Incrementar contador de fichas negras
        }
        else
        {
            team=1;
            blancas++; // Incrementar contador de fichas blancas
        }

        stone.GetComponent<MeshRenderer>().material=colors[team];

        Vector3 temp = new Vector3(x,0,y);
        stone.transform.position += temp;
    
        points[x,y].tag = color;
        stones[x,y] = stone;
        Point13 p = points[x,y].GetComponent<Point13>();
        p.x=stone.x;
        p.y=stone.y;
        p.stone13=stone;
        stone.transform.SetParent(points[x,y].transform);

        connectStones(p);
        checkNeighbors(p);

        UpdateScoreUI(); // Actualizar UI del marcador
        CheckEndGame();
        return stone;
    }

    public void setConnections(Point13 point)
    {
        Stone13 stone = point.gameObject.transform.GetChild(0).GetComponent<Stone13>();
        int x = stone.x;
        int y = stone.y;
        string team = point.gameObject.tag;

        List<Point13> neighbors = new List<Point13>();

        if (y+1 >= 0 && y+1 <= 12)
        {Point13 north = points[(int)x,(int)y+1].GetComponent<Point13>();neighbors.Add(north);}
        if (x+1 >= 0 && x+1 <= 12)
        {Point13 east = points[(int)x+1,(int)y].GetComponent<Point13>();neighbors.Add(east);}
        if (y-1 >= 0 && y-1 <= 12)
        {Point13 south = points[(int)x,(int)y-1].GetComponent<Point13>();neighbors.Add(south);}
        if (x-1 >= 0 && x-1 <= 12)
        {Point13 west = points[(int)x-1,(int)y].GetComponent<Point13>();neighbors.Add(west);}

        foreach (Point13 n in neighbors)
        {
            GameObject go = n.gameObject;
            if (go.tag == team)
            {
                if (!point.GetComponent<Point13>().connections.Contains(go.GetComponent<Point13>()))
                    point.GetComponent<Point13>().connections.Add(go.GetComponent<Point13>());
                if (!go.GetComponent<Point13>().connections.Contains(point.GetComponent<Point13>()))
                    go.GetComponent<Point13>().connections.Add(point.GetComponent<Point13>());
            }
        }
    }

    public HashSet<Point13> getGroup(Point13 point)
    {
        HashSet<Point13> visited = new HashSet<Point13>();
        
        if (point.connections.Contains(point))
                return visited;
            
        var queue = new Queue<Point13>();
        queue.Enqueue(point);

        while (queue.Count > 0)
        {
            var vertex = queue.Dequeue();

            if (visited.Contains(vertex))
                {continue;}

            visited.Add(vertex);
            if (!point.group.Contains(vertex))
                {
                    point.group.Add(vertex);
                    point.dupGroup.Add(vertex);
                }

            foreach(Point13 neighbor in vertex.connections)
                if (!visited.Contains(neighbor))
                    queue.Enqueue(neighbor);
        }

        return visited;
    }

    public void sendGroup(Point13 point)
    {
        foreach (Point13 member in point.group)
        {
            foreach (Point13 p in point.group)
            if (!member.group.Contains(p))
            {
               member.group.Add(p);
               member.dupGroup.Add(p);
            }
        }
    }

    public void getNeighbors(Point13 point)
    {
        if (point.y+1 < 13)
        {
            Point13 north = points[(int)point.x,(int)point.y+1].GetComponent<Point13>();
            point.neighbors.Add(north);
        }
        if (point.x+1 < 13)
        {
            Point13 east = points[(int)point.x+1,(int)point.y].GetComponent<Point13>();
            point.neighbors.Add(east);
        }
        if (point.y-1 > -1)
        {
            Point13 south = points[(int)point.x,(int)point.y-1].GetComponent<Point13>();
            point.neighbors.Add(south);
        }
        if (point.x-1 > -1)
        {
            Point13 west = points[(int)point.x-1,(int)point.y].GetComponent<Point13>();
            point.neighbors.Add(west);
        }
    }    

    public void getReach(Point13 point)
    {
        foreach (Point13 member in point.group)
            foreach( Point13 neighbor in member.neighbors)
                if (!point.reached.Contains(neighbor))
                    point.reached.Add(neighbor);
    }

    public void sendReach(Point13 point)
    {
        foreach (Point13 member in point.group)
            foreach (Point13 p in point.reached)
                if (!member.reached.Contains(p))
                    member.reached.Add(p);
    }

    public bool validGroup(Point13 point)
    {
        bool libertyFound = false;
        foreach (Point13 reach in point.reached)
            if (reach.gameObject.tag == empty)
                libertyFound = true;
        if (libertyFound)
            return true;
        else
            return false;
    }

    private void UpdateScoreUI()
    {
        negrasText.text = negras.ToString();
        blancasText.text = blancas.ToString();
        ganaBlancasText.text = ganablancas.ToString();
        ganaNegrasText.text = gananegras.ToString();
        totalBlancasText.text = (blancas + ganablancas).ToString();
        totalNegrasText.text = (negras + gananegras).ToString();

    }
    //public void killGroup(Point13 point)
    //{
    //    foreach (Point13 member in point.group)
    //    {
    //        Destroy(member.gameObject.transform.GetChild(0).gameObject);
    //        member.gameObject.tag = empty;
    //    }
    //}
    public void killGroup(Point13 point)
    {
        foreach (Point13 member in point.group)
        {
            Stone13 stone = member.stone13;
            if (stone.color == black)
            {
                negras--; // Disminuir contador de fichas negras
                ganablancas++;
                float randomX = Random.Range(zonaAleatoriaBlancas.x, zonaAleatoriaBlancas.x + 1f);
                float fixedY = .8f; // Cambia este valor según la posición Y deseada
                float randomZ = Random.Range(zonaAleatoriaBlancas.z, zonaAleatoriaBlancas.z + 1f);
                
                Vector3 posicionNuevoPrefab = transform.position + new Vector3(randomX, fixedY, randomZ);
                Quaternion rotacion = Quaternion.Euler(90f, 0f, 0f);
                // Instanciar el nuevo prefab en la nueva posición
                Instantiate(blancaGanada, posicionNuevoPrefab, rotacion);
                transform.localScale = new Vector3(25, 25, 25);

            }
            else if (stone.color == white)
            {
                blancas--; // Disminuir contador de fichas blancas
                gananegras++;
                float randomX2 = Random.Range(zonaAleatoriaNegras.x, zonaAleatoriaNegras.x + 1f);
                float fixedY2 = .8f; // Cambia este valor según la posición Y deseada
                float randomZ2 = Random.Range(zonaAleatoriaNegras.z, zonaAleatoriaNegras.z + 1f);
                Vector3 posicionNuevoPrefab2 = transform.position + new Vector3(randomX2, fixedY2, randomZ2);
                Quaternion rotacion = Quaternion.Euler(90f, 0f, 0f);
                // Instanciar el nuevo prefab en la nueva posición
                Instantiate(negraGanada, posicionNuevoPrefab2, rotacion);
               

            }


            Destroy(stone.gameObject);
            member.gameObject.tag = empty;

        }

        UpdateScoreUI(); // Actualizar UI del marcador
    }

    public void connectStones(Point13 point)
    {
        setConnections(point);
        getGroup(point);
        sendGroup(point);
        getNeighbors(point);
        getReach(point);
        sendReach(point);
    }

    public void checkNeighbors(Point13 point)
    {
        foreach (Point13 neighbor in point.neighbors)
            if (!validGroup(neighbor))
                killGroup(neighbor);
    }
    public void Rendir()
    {
        panelGameOver.SetActive(true);
        if ((negras + gananegras) > (blancas + ganablancas))
        {
            ganador.text = "Ganan NEGRAS";
            ganador.color = new Color(87, 87, 87, 10);
            finalNegrasText.text = (negras + gananegras).ToString();
            finalBlancasText.text = (blancas + ganablancas).ToString();
        }
        else
        {
            ganador.text = "Ganan BLANCAS";
            ganador.color = new Color(255, 255, 255, 255);
            finalBlancasText.text = (blancas + ganablancas).ToString();
            finalNegrasText.text = (negras + gananegras).ToString();
        }
    }
    private bool CheckEndGame()
    {
        // Recorre todas las posiciones del tablero
        for (int x = 0; x < BOARD_SIZE; x++)
        {
            for (int y = 0; y < BOARD_SIZE; y++)
            {
                // Verifica si la posición está vacía
                if (points[x, y].tag == empty)
                {
                    // Intenta colocar una piedra negra en la posición (x, y) y verifica si es un movimiento legal
                    if (IsLegalMove(black, x, y))
                    {
                        return false; // Si es un movimiento legal, el juego continúa
                    }

                    // Intenta colocar una piedra blanca en la posición (x, y) y verifica si es un movimiento legal
                    if (IsLegalMove(white, x, y))
                    {
                        return false; // Si es un movimiento legal, el juego continúa
                    }
                }
            }
        }
        panelGameOver.SetActive(true);
        return true; // Si no se encontraron movimientos legales en ninguna posición vacía, el juego termina

    }
    private bool IsLegalMove(string color, int x, int y)
    {
        // Verificar si la posición (x, y) está vacía
        if (points[x, y].tag != empty)
        {
            return false; // La posición no está vacía, por lo que no es un movimiento legal
        }

        // Verificar si colocar una piedra en la posición (x, y) capturaría alguna piedra del oponente
        bool capturesOpponentStone = false;

        // Verificar si colocar una piedra en la posición (x, y) captura piedras del oponente
        if (CheckCaptures(color, x, y))
        {
            capturesOpponentStone = true;
        }

        // Verificar la regla del suicidio
        if (!capturesOpponentStone && !CheckAliveGroup(color, x, y))
        {
            return false; // Colocar la piedra no crea un grupo vivo y no captura piedras del oponente, por lo que no es un movimiento legal
        }

        // Verificar la regla del Ko
        if (CheckKo(color, x, y))
        {
            return false; // Movimiento repetido, por lo que no es un movimiento legal
        }

        // Si no se encontraron problemas con el movimiento, entonces es legal
        return true;
    }
    private bool CheckCaptures(string color, int x, int y)
    {
        // Verificar si colocar una piedra en la posición (x, y) captura piedras del oponente en alguna dirección
        return CheckCapturesDirection(color, x, y, 1, 0) ||
               CheckCapturesDirection(color, x, y, -1, 0) ||
               CheckCapturesDirection(color, x, y, 0, 1) ||
               CheckCapturesDirection(color, x, y, 0, -1);
    }
    private bool CheckCapturesDirection(string color, int x, int y, int deltaX, int deltaY)
    {
        // Obtener el color del oponente
        string opponentColor = (color == black) ? white : black;

        // Calcular la nueva posición en la dirección específica
        int newX = x + deltaX;
        int newY = y + deltaY;

        // Verificar si la nueva posición está dentro del tablero
        if (!IsValidPosition(newX, newY))
        {
            return false; // La nueva posición está fuera del tablero, no hay capturas en esta dirección
        }

        // Verificar si hay una piedra del oponente en la nueva posición
        if (points[newX, newY].tag != opponentColor)
        {
            return false; // No hay piedra del oponente en la nueva posición, no hay capturas en esta dirección
        }

        // Verificar si la piedra del oponente en la nueva posición sería capturada
        if (WouldCapture(opponentColor, newX, newY, deltaX, deltaY))
        {
            return true; // La piedra del oponente en la nueva posición sería capturada, hay capturas en esta dirección
        }

        return false; // No hay capturas en esta dirección
    }

    private bool IsValidPosition(int x, int y)
    {
        // Verificar si la posición (x, y) está dentro del tablero
        return x >= 0 && x < BOARD_SIZE && y >= 0 && y < BOARD_SIZE;
    }

    private bool WouldCapture(string color, int x, int y, int deltaX, int deltaY)
    {
        // Calcular la nueva posición en la dirección específica
        int newX = x + deltaX;
        int newY = y + deltaY;

        // Verificar si la nueva posición está dentro del tablero
        if (!IsValidPosition(newX, newY))
        {
            return false; // La nueva posición está fuera del tablero, no hay capturas en esta dirección
        }

        // Verificar si la nueva posición está vacía
        if (points[newX, newY].tag == empty)
        {
            return false; // La nueva posición está vacía, no hay capturas en esta dirección
        }

        // Verificar si la nueva posición contiene una piedra del mismo color
        if (points[newX, newY].tag == color)
        {
            return true; // La nueva posición contiene una piedra del mismo color, la piedra del oponente sería capturada
        }

        // Si la nueva posición contiene una piedra del oponente, continuar verificando en esa dirección
        return WouldCapture(color, newX, newY, deltaX, deltaY);
    }




    private bool CheckLiberty(int x, int y)
    {
        // Verificar si la piedra en la posición (x, y) tiene libertad
        // Implementa tu lógica para verificar si la piedra en la posición (x, y) tiene al menos una libertad
        // Por simplicidad, podrías verificar si al menos una de las posiciones adyacentes a (x, y) está vacía
        return true; // Por simplicidad, siempre retorna true en esta implementación básica
    }

    private bool CheckAliveGroup(string color, int x, int y)
    {
        // Verifica si colocar una piedra del color dado en la posición (x, y) crearía un grupo vivo
        // Implementa tu lógica para verificar si la piedra colocada formaría un grupo vivo
        // Por simplicidad, podrías verificar si la piedra colocada no estaría en estado de "suicidio"
        return true; // Por simplicidad, siempre retorna true en esta implementación básica
    }
    private bool CheckKo(string color, int x, int y)
    {
        // Verificar la regla del Ko para evitar movimientos repetidos
        // Implementa tu lógica para verificar si el movimiento en la posición (x, y) viola la regla del Ko
        return false; // Por simplicidad, siempre retorna false en esta implementación básica
    }
}

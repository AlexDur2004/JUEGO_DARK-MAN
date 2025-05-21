using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

/// <summary>
/// Clase encargada de generar y gestionar los objetos decorativos del laberinto.
/// </summary>
/// <remarks>
/// Esta clase utiliza el patrón Singleton para asegurar una única instancia que maneja
/// la generación, posicionamiento y reciclaje de objetos decorativos dentro del laberinto.
/// Implementa un sistema de distribución espacial para evitar concentración de objetos
/// y detecta colisiones con paredes para un posicionamiento realista.
/// </remarks>
public class SpawnerDecoraciones : Singleton<SpawnerDecoraciones>
{    /// <summary>
    /// Configuración de los elementos decorativos del laberinto
    /// </summary>
    [Header("Configuración de Decoración")]
    
    /// <summary>
    /// Lista de objetos decorativos que pueden ser generados en el laberinto
    /// </summary>
    [SerializeField] private List<ObjetoDecorativo> objetosDecorativos = new List<ObjetoDecorativo>();
    
    /// <summary>
    /// Probabilidad base de generar un objeto en una celda (rango 0-1)
    /// </summary>
    [SerializeField] private float probabilidadSpawnObjeto = 0.6f;
    
    /// <summary>
    /// Distancia mínima uniforme para todos los objetos respecto a las paredes
    /// </summary>
    [SerializeField] private float distanciaMinimaAPared = 1.5f;
    
    /// <summary>
    /// Máximo número de objetos decorativos permitido por celda
    /// </summary>
    [SerializeField] private int maxObjetosPorCelda = 1;
    
    /// <summary>
    /// Distancia mínima en celdas entre objetos del mismo tipo
    /// </summary>
    [SerializeField] private int distanciaMinEntreObjetos = 4;
    
    /// <summary>
    /// Contenedor para organizar jerárquicamente los objetos decorativos
    /// </summary>
    private Transform contenedorDecorativos;
    
    /// <summary>
    /// Pool de objetos para reciclar GameObjects decorativos
    /// </summary>
    private ObjectPool<GameObject> poolDecorativos = new ObjectPool<GameObject>();
    
    /// <summary>
    /// Diccionario que registra las últimas posiciones de objetos por tipo
    /// </summary>
    /// <remarks>
    /// La clave es el índice del objeto en la lista objetosDecorativos
    /// El valor es una lista de coordenadas donde se han colocado objetos de ese tipo
    /// </remarks>
    private Dictionary<int, List<Vector2>> ultimasPosicionesObjetos = new Dictionary<int, List<Vector2>>();
    
    /// <summary>
    /// Contador total de objetos decorativos generados
    /// </summary>
    private int totalObjetosGenerados = 0;

    /// <summary>
    /// Referencia al sistema generador del laberinto
    /// </summary>
    private SpawnerLaberinto spawnerLaberinto;    /// <summary>
    /// Clase que define un objeto decorativo para el laberinto
    /// </summary>
    /// <remarks>
    /// Esta clase encapsula las propiedades de un tipo de objeto decorativo,
    /// incluyendo su representación visual, probabilidad de aparición y orientación.
    /// </remarks>
    [System.Serializable]
    public class ObjetoDecorativo
    {
        /// <summary>
        /// Prefab que representa visualmente el objeto decorativo
        /// </summary>
        public GameObject prefab;

        /// <summary>
        /// Probabilidad individual de aparición del objeto (0-1)
        /// </summary>
        /// <remarks>
        /// Este valor se multiplica por la probabilidad general de spawn
        /// </remarks>
        public float probabilidadAparicion = 1f;

        /// <summary>
        /// Rotación adicional aplicada al objeto al instanciarse
        /// </summary>
        public Vector3 rotacionAdicional = Vector3.zero;

        /// <summary>
        /// Constructor para crear objetos con inicialización directa
        /// </summary>
        /// <param name="prefab">Prefab del objeto decorativo</param>
        /// <param name="probabilidad">Probabilidad de aparición (0-1)</param>
        public ObjetoDecorativo(GameObject prefab, float probabilidad = 1f)
        {
            this.prefab = prefab;
            this.probabilidadAparicion = probabilidad;
        }
    }    /// <summary>
    /// Inicializa el sistema de decoración del laberinto
    /// </summary>
    /// <remarks>
    /// Crea el contenedor jerárquico para los objetos decorativos y
    /// obtiene la referencia necesaria al SpawnerLaberinto
    /// </remarks>
    protected override void Awake()
    {
        base.Awake();
        contenedorDecorativos = new GameObject("Decorativos").transform;
        contenedorDecorativos.parent = transform;

        // Obtener la referencia al SpawnerLaberinto (se requiere para instanciar objetos)
        spawnerLaberinto = GetComponent<SpawnerLaberinto>();
        if (spawnerLaberinto == null)
        {
            spawnerLaberinto = GetComponentInParent<SpawnerLaberinto>();
        }
    }    /// <summary>
    /// Limpia todas las decoraciones del laberinto
    /// </summary>
    /// <remarks>
    /// Desactiva todos los objetos decorativos y reinicia el historial de posiciones
    /// </remarks>
    public void LimpiarDecoraciones()
    {
        poolDecorativos.DesactivarTodos();
        LimpiarHistorialPosiciones();
    }

    /// <summary>
    /// Limpia solo el historial de posiciones de objetos
    /// </summary>
    /// <remarks>
    /// Reinicia el registro de posiciones pero mantiene los objetos activos en el laberinto
    /// </remarks>
    public void LimpiarHistorialPosiciones()
    {
        ultimasPosicionesObjetos.Clear();
        totalObjetosGenerados = 0;
    }    /// <summary>
    /// Genera objetos decorativos para una celda del laberinto
    /// </summary>
    /// <param name="celda">La celda del laberinto donde colocar decoraciones</param>
    /// <param name="posicionCelda">Posición 3D de la celda en el mundo</param>
    /// <param name="anchoCelda">Ancho de la celda en unidades de mundo</param>
    /// <remarks>
    /// Este método evalúa la celda y aplica una serie de reglas para determinar
    /// qué objetos decorativos colocar y dónde. Respeta los límites de objetos por celda,
    /// las probabilidades configuradas y las restricciones de distancia.
    /// </remarks>
    public void GenerarObjetosDecorativos(CeldaLaberinto celda, Vector3 posicionCelda, float anchoCelda)
    {
        if (objetosDecorativos.Count == 0) return;

        // Obtén las coordenadas de la celda actual
        Vector2 coordenadasCelda = new Vector2(
            Mathf.RoundToInt(posicionCelda.x / anchoCelda), Mathf.RoundToInt(posicionCelda.z / anchoCelda)
        );

        int objetosGenerados = 0;
        bool tienePared = CeldaTieneParedes(celda);

        // Si la celda tiene paredes y es muy pequeña, no generar decoraciones
        if (tienePared && anchoCelda < distanciaMinimaAPared * 3)
            return;

        float randomProbSpawnGeneral = Random.value;
        // Crear y mezclar la lista de índices (preservando índice 0)
        List<int> indicesObjetos = ObtenerIndicesAleatorios(objetosDecorativos.Count);

        foreach (int i in indicesObjetos)
        {
            if (objetosGenerados >= maxObjetosPorCelda) break;

            ObjetoDecorativo objetoDecorativo = objetosDecorativos[i];

            // Verificar todas las condiciones que impiden generar el objeto
            if (objetoDecorativo.prefab == null ||
                !PuedePosicionarObjetoAqui(i, coordenadasCelda) ||
                (i > 0 && randomProbSpawnGeneral > probabilidadSpawnObjeto) ||
                Random.value > objetoDecorativo.probabilidadAparicion)
            {
                continue;
            }

            if (IntentarColocarObjeto(objetoDecorativo, celda, posicionCelda, coordenadasCelda, tienePared, anchoCelda, i))
            {
                objetosGenerados++;
                totalObjetosGenerados++;
            }
        }
    }    /// <summary>
    /// Instancia o reutiliza un GameObject desde el pool de objetos
    /// </summary>
    /// <param name="prefab">Prefab a instanciar</param>
    /// <param name="posicion">Posición donde colocar el objeto</param>
    /// <param name="rotacion">Rotación a aplicar al objeto</param>
    /// <returns>GameObject instanciado o reutilizado</returns>
    /// <remarks>
    /// Delega la tarea al método equivalente en SpawnerLaberinto para mantener coherencia
    /// y evitar duplicación de código
    /// </remarks>
    private GameObject InstanciarOReutilizar(GameObject prefab, Vector3 posicion, Quaternion rotacion)
    {
        // Usar el método del SpawnerLaberinto para evitar duplicación de código
        return spawnerLaberinto.InstanciarOReutilizar(prefab, posicion, rotacion, contenedorDecorativos, poolDecorativos);
    }

    /// <summary>
    /// Registra la posición donde se ha colocado un objeto decorativo
    /// </summary>
    /// <param name="indiceObjeto">Índice del objeto en la lista de decorativos</param>
    /// <param name="coordenadasCelda">Coordenadas de la celda donde se colocó</param>
    /// <remarks>
    /// Este registro se utiliza para mantener distancias mínimas entre objetos del mismo tipo
    /// </remarks>
    private void RegistrarPosicionObjeto(int indiceObjeto, Vector2 coordenadasCelda)
    {
        if (!ultimasPosicionesObjetos.ContainsKey(indiceObjeto))
        {
            ultimasPosicionesObjetos[indiceObjeto] = new List<Vector2>();
        }
        ultimasPosicionesObjetos[indiceObjeto].Add(coordenadasCelda);
    }    /// <summary>
    /// Determina si se puede posicionar un objeto en una celda específica
    /// </summary>
    /// <param name="indiceObjeto">Índice del objeto a comprobar</param>
    /// <param name="coordenadasCelda">Coordenadas de la celda candidata</param>
    /// <returns>True si se puede colocar un objeto, False en caso contrario</returns>
    /// <remarks>
    /// Implementa la verificación de distancias entre objetos del mismo tipo
    /// para asegurar una distribución espacial visualmente equilibrada.
    /// El objeto con índice 0 recibe tratamiento especial, pudiendo colocarse sin restricciones.
    /// </remarks>
    private bool PuedePosicionarObjetoAqui(int indiceObjeto, Vector2 coordenadasCelda)
    {
        // El primer objeto siempre se puede colocar en cualquier parte
        if (indiceObjeto == 0) return true;

        // Calcular el cuadrado de la distancia mínima para comparaciones más eficientes
        int distanciaCuadrado = distanciaMinEntreObjetos * distanciaMinEntreObjetos;

        // Verificar la distancia a todos los objetos ya colocados (excepto el tipo 0)
        foreach (var par in ultimasPosicionesObjetos)
        {
            if (par.Key == 0) continue;

            // Comprobar cada posición de objeto existente
            foreach (var pos in par.Value)
            {
                if ((pos - coordenadasCelda).sqrMagnitude < distanciaCuadrado)
                    return false;
            }
        }

        return true;
    }    /// <summary>
    /// Detecta si existe colisión entre un objeto y las paredes de una celda
    /// </summary>
    /// <param name="celda">Celda del laberinto donde se valida la colisión</param>
    /// <param name="objeto">GameObject cuya colisión se comprueba</param>
    /// <param name="posicion">Posición propuesta para el objeto</param>
    /// <param name="rotacion">Rotación propuesta para el objeto</param>
    /// <returns>True si hay colisión con paredes, False si la posición es segura</returns>
    /// <remarks>
    /// Utiliza los colliders del objeto para comprobar matemáticamente si colisionaría
    /// con las paredes, añadiendo un margen de seguridad para evitar objetos demasiado cercanos.
    /// </remarks>
    private bool DetectarColisionConPared(CeldaLaberinto celda, GameObject objeto, Vector3 posicion, Quaternion rotacion)
    {
        // Optimización rápida: si la celda no tiene paredes, no hay colisión
        if (!CeldaTieneParedes(celda))
            return false;

        // Obtener los colliders del objeto
        BoxCollider[] boxColliders = objeto.GetComponentsInChildren<BoxCollider>();
        if (boxColliders.Length == 0)
            return false;

        // Usar un margen de seguridad para evitar objetos demasiado pegados a las paredes
        const float margenSeguridad = 0.6f;
        Vector3 margenVector = Vector3.one * margenSeguridad;

        // Verificar cada collider
        foreach (BoxCollider boxCollider in boxColliders)
        {
            // Calcular tamaño y posición del collider en el mundo
            Vector3 escalaObjeto = objeto.transform.lossyScale;
            Vector3 tamanioReal = Vector3.Scale(boxCollider.size, escalaObjeto);

            // Crear matriz de transformación para convertir posición local a mundial
            Matrix4x4 matrizTransformacion = Matrix4x4.TRS(posicion, rotacion, escalaObjeto);
            Vector3 centroWorldSpace = matrizTransformacion.MultiplyPoint(boxCollider.center);

            // Verificar si hay colisión con algún otro objeto (paredes)
            if (Physics.CheckBox(centroWorldSpace, tamanioReal / 2 + margenVector, rotacion))
                return true;
        }

        // No se detectó colisión
        return false;
    }    /// <summary>
    /// Verifica si una celda tiene paredes en alguna de sus cuatro direcciones
    /// </summary>
    /// <param name="celda">Celda del laberinto a comprobar</param>
    /// <returns>True si la celda tiene al menos una pared, False si no tiene ninguna</returns>
    private bool CeldaTieneParedes(CeldaLaberinto celda) => celda != null &&
        (celda.ParedIzquierda || celda.ParedDerecha || celda.ParedSuperior || celda.ParedInferior);    /// <summary>
    /// Aplica una rotación adicional a una rotación base
    /// </summary>
    /// <param name="rotacionBase">Rotación base inicial</param>
    /// <param name="rotacionAdicional">Rotación adicional a aplicar</param>
    /// <returns>Nueva rotación combinada</returns>
    /// <remarks>
    /// Asegura que los ángulos en el eje Y siempre sean impares para proporcionar
    /// mayor variedad visual en el laberinto.
    /// </remarks>
    private Quaternion AplicarRotacionAdicional(Quaternion rotacionBase, Vector3 rotacionAdicional)
    {
        if (rotacionAdicional != Vector3.zero)
        {
            // Ajustar la rotación en Y para que sea impar si no lo es
            Vector3 rotacionAjustada = rotacionAdicional;
            if (Mathf.RoundToInt(rotacionAjustada.y) % 2 == 0)
            {
                rotacionAjustada.y += 1;
            }

            return rotacionBase * Quaternion.Euler(rotacionAjustada);
        }
        return rotacionBase;
    }    /// <summary>
    /// Prueba diferentes rotaciones de un objeto en una posición dada hasta encontrar una sin colisiones
    /// </summary>
    /// <param name="celda">Celda del laberinto donde se prueba el objeto</param>
    /// <param name="objetoPrefab">Prefab del objeto a probar</param>
    /// <param name="posicion">Posición donde se prueba el objeto</param>
    /// <param name="rotacionAdicional">Rotación adicional a aplicar</param>
    /// <param name="rotacionResultante">Variable de salida con la rotación encontrada</param>
    /// <returns>True si se encontró una rotación válida, False si todas colisionan</returns>
    /// <remarks>
    /// Utiliza ángulos impares para dar más variedad visual.
    /// Prueba sistemáticamente 8 orientaciones diferentes (1°, 45°, 91°, etc.)
    /// </remarks>
    private bool ProbarRotacionesObjeto(CeldaLaberinto celda, GameObject objetoPrefab, Vector3 posicion, Vector3 rotacionAdicional, ref Quaternion rotacionResultante)
    {
        // Usamos 8 ángulos diferentes con valores impares para mayor variedad
        int[] angulosImpares = { 1, 45, 91, 135, 181, 225, 271, 315 };

        for (int i = 0; i < angulosImpares.Length; i++)
        {
            float angulo = angulosImpares[i];
            Quaternion nuevaRotacion = AplicarRotacionAdicional(Quaternion.Euler(0, angulo, 0), rotacionAdicional);

            if (!DetectarColisionConPared(celda, objetoPrefab, posicion, nuevaRotacion))
            {
                rotacionResultante = nuevaRotacion;
                return true;
            }
        }
        return false;
    }    /// <summary>
    /// Genera una lista de índices mezclados aleatoriamente, preservando el índice 0
    /// </summary>
    /// <param name="cantidad">Número de índices a generar</param>
    /// <returns>Lista de índices aleatorizados (excepto el índice 0)</returns>
    /// <remarks>
    /// Implementa una variante del algoritmo de Fisher-Yates que preserva
    /// el índice 0 en su posición original y sólo mezcla los índices restantes.
    /// Esto permite dar un tratamiento especial al primer objeto decorativo.
    /// </remarks>
    private List<int> ObtenerIndicesAleatorios(int cantidad)
    {
        // Crear e inicializar lista
        List<int> indices = new List<int>(cantidad);
        for (int i = 0; i < cantidad; i++)
            indices.Add(i);

        // Algoritmo de Fisher-Yates modificado para preservar índice 0
        // Solo mezclamos desde el índice 1 en adelante
        for (int i = indices.Count - 1; i > 1; i--)
        {
            int j = Random.Range(1, i + 1);

            // Intercambiar usando deconstrucción de tupla
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }

        return indices;
    }    /// <summary>
    /// Intenta colocar un objeto decorativo en una celda del laberinto
    /// </summary>
    /// <param name="objetoDecorativo">Objeto decorativo a colocar</param>
    /// <param name="celda">Celda donde intentar colocar el objeto</param>
    /// <param name="posicionCelda">Posición 3D de la celda en el mundo</param>
    /// <param name="coordenadasCelda">Coordenadas 2D de la celda en la matriz del laberinto</param>
    /// <param name="tienePared">Indica si la celda tiene paredes</param>
    /// <param name="anchoCelda">Ancho de la celda en unidades de mundo</param>
    /// <param name="indiceObjeto">Índice del objeto en la lista de decorativos</param>
    /// <returns>True si se logró colocar el objeto, False si no fue posible</returns>
    /// <remarks>
    /// Este método orquesta la secuencia completa para colocar un objeto:
    /// busca una posición libre, aplica las rotaciones adecuadas, instancia el objeto
    /// y registra su posición en el historial.
    /// </remarks>
    private bool IntentarColocarObjeto(ObjetoDecorativo objetoDecorativo, CeldaLaberinto celda,
                                      Vector3 posicionCelda, Vector2 coordenadasCelda,
                                      bool tienePared, float anchoCelda, int indiceObjeto)
    {
        Vector3 posicionObjeto = posicionCelda;
        Quaternion rotacionObjeto = Quaternion.identity;
        bool posicionValida = false;

        // Siempre usar posiciones libres lejos de las paredes
        posicionValida = ObtenerPosicionLibre(celda, ref posicionObjeto, ref rotacionObjeto, objetoDecorativo, posicionCelda, tienePared, anchoCelda);
        // Si no se encontró una posición válida, no seguir
        if (!posicionValida) return false;

        // Aplicar la posición Y desde el prefab original
        float posY = objetoDecorativo.prefab.transform.position.y;
        Vector3 posicionFinal = new Vector3(posicionObjeto.x, posY, posicionObjeto.z);

        // Crear el objeto y registrar su posición
        InstanciarOReutilizar(objetoDecorativo.prefab, posicionFinal, rotacionObjeto);
        RegistrarPosicionObjeto(indiceObjeto, coordenadasCelda);
        return true;
    }    /// <summary>
    /// Obtiene una posición libre para un objeto decorativo alejada de las paredes
    /// </summary>
    /// <param name="celda">Celda donde buscar la posición</param>
    /// <param name="posicionObjeto">Variable de salida con la posición encontrada</param>
    /// <param name="rotacionObjeto">Variable de salida con la rotación encontrada</param>
    /// <param name="objetoDecorativo">Objeto decorativo a colocar</param>
    /// <param name="posicionCelda">Posición central de la celda</param>
    /// <param name="tienePared">Indica si la celda tiene paredes</param>
    /// <param name="anchoCelda">Ancho de la celda en unidades de mundo</param>
    /// <returns>True si se encontró una posición válida, False si no</returns>
    /// <remarks>
    /// Utiliza diferentes estrategias dependiendo de si la celda tiene paredes:
    /// - Sin paredes: posición aleatoria dentro del área útil
    /// - Con paredes: intenta varias posiciones predefinidas hasta encontrar una sin colisiones
    /// </remarks>
    private bool ObtenerPosicionLibre(CeldaLaberinto celda, ref Vector3 posicionObjeto, ref Quaternion rotacionObjeto,
                                     ObjetoDecorativo objetoDecorativo, Vector3 posicionCelda, bool tienePared, float anchoCelda)
    {
        // Calcular el área segura lejos de las paredes
        float espacioUtilizable = anchoCelda - (distanciaMinimaAPared * 2);

        // Verificar si hay suficiente espacio
        if (espacioUtilizable <= 0.5f)
            return false;
            
        // Si no hay paredes, posicionar directamente en el centro con una rotación aleatoria
        if (!tienePared)
        {
            // Generar posición y rotación iniciales
            float offsetX = Random.Range(-espacioUtilizable / 2, espacioUtilizable / 2);
            float offsetZ = Random.Range(-espacioUtilizable / 2, espacioUtilizable / 2);
            posicionObjeto = posicionCelda + new Vector3(offsetX, 0, offsetZ);

            // Generar un ángulo impar aleatorio para la rotación
            int anguloBase = Random.Range(0, 360);
            int anguloImpar = (anguloBase % 2 == 0) ? anguloBase + 1 : anguloBase;

            rotacionObjeto = AplicarRotacionAdicional(
                Quaternion.Euler(0, anguloImpar, 0),
                objetoDecorativo.rotacionAdicional
            );
            return true;
        }        // Si hay paredes, buscar una posición válida
        // Primero intentar con ubicación aleatoria
        float offsetX2 = Random.Range(-espacioUtilizable / 2, espacioUtilizable / 2);
        float offsetZ2 = Random.Range(-espacioUtilizable / 2, espacioUtilizable / 2);
        posicionObjeto = posicionCelda + new Vector3(offsetX2, 0, offsetZ2);

        // Probar rotaciones alternativas en la posición aleatoria
        if (ProbarRotacionesObjeto(celda, objetoDecorativo.prefab, posicionObjeto, objetoDecorativo.rotacionAdicional, ref rotacionObjeto))
            return true;

        // Si no funciona, probar posiciones predefinidas
        // Usar 5 posiciones: centro y offset en 4 direcciones
        Vector3[] posicionesAlternativas = {
            posicionCelda,                                             // Centro
            posicionCelda + new Vector3(anchoCelda / 6, 0, 0),         // Derecha 
            posicionCelda + new Vector3(-anchoCelda / 6, 0, 0),        // Izquierda
            posicionCelda + new Vector3(0, 0, anchoCelda / 6),         // Arriba
            posicionCelda + new Vector3(0, 0, -anchoCelda / 6)         // Abajo
        };

        // Probar cada posición alternativa con diferentes rotaciones
        foreach (Vector3 nuevaPos in posicionesAlternativas)
        {
            if (ProbarRotacionesObjeto(celda, objetoDecorativo.prefab, nuevaPos, objetoDecorativo.rotacionAdicional, ref rotacionObjeto))
            {
                posicionObjeto = nuevaPos;
                return true;
            }
        }

        // No se encontró una posición válida
        return false;
    }    /// <summary>
    /// Limpia los objetos temporales cuando se destruye el script
    /// </summary>
    /// <remarks>
    /// Asegura que se libere apropiadamente la memoria del pool de objetos
    /// cuando el componente es destruido
    /// </remarks>
    protected override void OnDestroy()
    {
        base.OnDestroy();

        // Limpiar el pool
        poolDecorativos.Clear();
    }
}
using UnityEngine;

namespace protogax.Tiler {
    public class TilerWorld : MonoBehaviour
    {
        [field: SerializeField]
        public int Rows {get; private set;} = 10;
        [field: SerializeField]
        public int Columns {get; private set;} = 10;
        [field: SerializeField]
        public float Size {get; private set;} = 1f;
        [field: SerializeField]
        public GameObject TilePrefab {get; private set;}

        public Vector3 Origin => transform.position - new Vector3(Rows * Size / 2f, 0, Columns * Size / 2f);
    }
}


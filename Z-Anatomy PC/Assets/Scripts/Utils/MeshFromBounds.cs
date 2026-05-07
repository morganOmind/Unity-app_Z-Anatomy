using UnityEngine;

public class MeshFromBounds {
    public static Mesh CreateMeshFromBounds(Vector3 center, Vector3 extends) {
        
        var xm_ym_zm = new Vector3(center.x - extends.x, center.y - extends.y, center.z - extends.z);
        var xp_ym_zm = new Vector3(center.x + extends.x, center.y - extends.y, center.z - extends.z);
        var xm_yp_zm = new Vector3(center.x - extends.x, center.y + extends.y, center.z - extends.z);
        var xp_yp_zm = new Vector3(center.x + extends.x, center.y + extends.y, center.z - extends.z);
        var xm_ym_zp = new Vector3(center.x - extends.x, center.y - extends.y, center.z + extends.z);
        var xp_ym_zp = new Vector3(center.x + extends.x, center.y - extends.y, center.z + extends.z);
        var xm_yp_zp = new Vector3(center.x - extends.x, center.y + extends.y, center.z + extends.z);
        var xp_yp_zp = new Vector3(center.x + extends.x, center.y + extends.y, center.z + extends.z);

        Vector3[] vertices = new Vector3[] {
        xm_ym_zm,
        xp_ym_zm,
        xm_yp_zm,
        xp_yp_zm,
        xm_ym_zp,
        xp_ym_zp,
        xm_yp_zp,
        xp_yp_zp
    };

        int[] indices = new int[] {
        0, 4, 2,
        2, 4, 6,
        1, 3, 5,
        3, 7, 5,
        0, 1, 4,
        1, 5, 4,
        2, 6, 3,
        3, 6, 7,
        0, 2, 1,
        1, 2, 3,
        4, 5, 6,
        5, 7, 6
    };

        var mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = indices;
        return mesh;
    }
}


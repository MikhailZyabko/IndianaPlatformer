using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ImageEffectAllowedInSceneView]
[ExecuteInEditMode]
public class Light2D : MonoBehaviour
{
    private Material _pointLightMat;
    private RenderTexture _mask;
    private Mesh _templateMesh;

    [SerializeField]
    private Color _ambientColor;
    public Color AmbientColor => _ambientColor;

    [SerializeField]
    private Texture _standartPointLightCookie;

    public static List<PointLight2D> pointLightList = new List<PointLight2D>();

    void OnEnable()
    {
        InitializeMaterials();
        InitializeMesh();
    }

    void OnDisable()
    {
        CommandBuffer buff = new CommandBuffer();
        buff.name = "Light Mask";

        _mask = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.BGRA32);
        buff.SetRenderTarget(_mask);
        buff.ClearRenderTarget(false, true, Color.white);

        buff.SetGlobalTexture("_LightMaskTexture", _mask);
        Graphics.ExecuteCommandBuffer(buff);
        RenderTexture.ReleaseTemporary(_mask);
    }

    void OnPreRender()
    {
        Camera cam = Camera.current;
        this.gameObject.SetActive(false);

        CommandBuffer buff = new CommandBuffer();
        buff.name = "Light Mask";

        _mask = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.BGRA32);
        buff.SetRenderTarget(_mask);
        buff.ClearRenderTarget(false, true, _ambientColor);

        buff.SetGlobalMatrix("unity_MatrixVP", GL.GetGPUProjectionMatrix(cam.projectionMatrix, true) * cam.worldToCameraMatrix);

        MaterialPropertyBlock pLightBlock = new MaterialPropertyBlock();
        foreach (var pointLight in pointLightList)
        {
            pLightBlock.SetColor("_LightColor", pointLight.color);
            pLightBlock.SetFloat("_Radius", pointLight.radius);
            if(pointLight.cookie == null)
                pLightBlock.SetTexture("_CookieText", _standartPointLightCookie);
            else
                pLightBlock.SetTexture("_CookieText", pointLight.cookie);

            buff.DrawMesh(_templateMesh, pointLight.transform.localToWorldMatrix, _pointLightMat, 0, -1, pLightBlock);
        }

        buff.SetGlobalTexture("_LightMaskTexture", _mask);
        Graphics.ExecuteCommandBuffer(buff);

        RenderTexture.ReleaseTemporary(_mask);
        this.gameObject.SetActive(true);
    }

    void InitializeMaterials()
    {
        _pointLightMat = new Material(Shader.Find("Hidden/PointLight2D"));
    }

    void InitializeMesh()
    {
        Vector3[] verticies = new Vector3[4];

        verticies[0] = new Vector3( 1.41421356237f,  1.41421356237f, 0.0f);
        verticies[1] = new Vector3( 1.41421356237f, -1.41421356237f, 0.0f);
        verticies[2] = new Vector3(-1.41421356237f,  1.41421356237f, 0.0f);
        verticies[3] = new Vector3(-1.41421356237f, -1.41421356237f, 0.0f);

        int[] triangles = new int[6];

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        triangles[3] = 1;
        triangles[4] = 3;
        triangles[5] = 2;

        Vector2[] uv = new Vector2[4];

        uv[0] = new Vector2(1.0f, 1.0f);
        uv[1] = new Vector2(1.0f, 0.0f);
        uv[2] = new Vector2(0.0f, 1.0f);
        uv[3] = new Vector2(0.0f, 0.0f);

        _templateMesh = new Mesh();
        _templateMesh.vertices = verticies;
        _templateMesh.triangles = triangles;
        _templateMesh.uv = uv;
    }
}

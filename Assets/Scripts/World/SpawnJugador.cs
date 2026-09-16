using System;
using UnityEngine;

[DefaultExecutionOrder(-50)]
public class SpawnJugador : MonoBehaviour
{
    static readonly string[] TakesMike = { "Espera", "Camina", "Corre", "Salta", "Gana", "Pierde" };

    public GameObject mikePrefab;

    void Start()
    {
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        PuntoSpawn(out Vector3 pos, out Quaternion rot);

        GameObject source = mikePrefab != null
            ? mikePrefab
            : ModelLoader.Load("Models/Mike/Mike@Espera", "Assets/Resources/Models/Mike/Mike@Espera.FBX");

        GameObject player;
        if (source != null)
            player = Instantiate(source, pos, rot);
        else
        {
            player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.transform.position = pos;
            player.GetComponent<Renderer>().material.color = new Color(0.45f, 0.85f, 0.25f);
        }

        player.name = "Mike";
        player.tag = "Player";

        if (!player.TryGetComponent(out CharacterController cc))
            cc = player.AddComponent<CharacterController>();
        cc.height = 2f;
        cc.center = new Vector3(0, 1f, 0);
        cc.radius = 0.4f;
        cc.detectCollisions = true;
        cc.skinWidth = 0.08f;

        AsegurarAnimacion(player);
        ReconectarMateriales.EnMike(player);
        AmbienteVisual.AsignarPupila(player);

        var tpc = player.GetComponent<ThirdPersonController>();
        if (tpc == null)
            tpc = player.AddComponent<ThirdPersonController>();
        else
            tpc.RecargarClips();
        if (player.GetComponent<MouseOrbit>() == null)
            player.AddComponent<MouseOrbit>();
        if (player.GetComponent<ObtieneObjeto>() == null)
            player.AddComponent<ObtieneObjeto>();
        if (player.GetComponent<ControlCursor>() == null)
            player.AddComponent<ControlCursor>();
        if (player.GetComponent<ObjetosPorJugador>() == null)
            player.AddComponent<ObjetosPorJugador>();
        if (player.GetComponent<NombreJugador>() == null)
            player.AddComponent<NombreJugador>();
        if (player.GetComponent<EtiquetaJugador>() == null)
            player.AddComponent<EtiquetaJugador>();

        var manager = GameObject.FindGameObjectWithTag("GameManager");
        if (manager != null)
        {
            var multi = manager.GetComponent<GestionaMultiJugador>();
            var nombre = player.GetComponent<NombreJugador>();
            nombre.nombreJugador = multi != null ? multi.nombreJugador : "Mike";
            nombre.fNombreSeteado = true;
        }
    }

    public static void PuntoSpawn(out Vector3 pos, out Quaternion rot)
    {
        var puntos = GameObject.FindGameObjectsWithTag("SpawnMike");
        Transform spawn = puntos.Length > 0 ? puntos[UnityEngine.Random.Range(0, puntos.Length)].transform : null;
        pos = spawn != null ? spawn.position : new Vector3(0, 1, -14);
        rot = spawn != null ? spawn.rotation : Quaternion.identity;
    }

    public static void Recolocar(Transform player)
    {
        if (player == null)
            return;

        PuntoSpawn(out Vector3 pos, out Quaternion rot);
        var cc = player.GetComponent<CharacterController>();
        if (cc != null)
            cc.enabled = false;
        player.SetPositionAndRotation(pos, rot);
        if (cc != null)
            cc.enabled = true;
    }

    static void AsegurarAnimacion(GameObject player)
    {
        var animator = player.GetComponent<Animator>();
        if (animator != null)
            Destroy(animator);

        var anim = player.GetComponent<Animation>();
        if (anim == null)
            anim = player.GetComponentInChildren<Animation>();
        if (anim == null)
            anim = player.AddComponent<Animation>();

        foreach (var take in TakesMike)
        {
            if (BuscarClip(anim, take) != null)
                continue;
            var clip = CargarClipMike(take);
            if (clip == null)
                continue;
            anim.AddClip(clip, take);
            var state = anim[take];
            if (state != null)
            {
                bool loop = take == "Espera" || take == "Camina" || take == "Corre";
                state.wrapMode = loop ? WrapMode.Loop : WrapMode.Once;
                if (take == "Corre")
                    state.speed = 1.75f;
            }
        }

        var espera = BuscarClip(anim, "Espera") ?? PrimerClip(anim);
        if (espera == null)
            return;

        if (anim.GetClip(espera.name) == null)
            anim.AddClip(espera, espera.name);
        anim.clip = espera;
        anim.wrapMode = WrapMode.Loop;
        anim.Play(espera.name);
    }

    static AnimationClip CargarClipMike(string take)
    {
        var clips = Resources.LoadAll<AnimationClip>("Models/Mike/Mike@" + take);
        if (clips != null)
        {
            foreach (var clip in clips)
            {
                if (clip != null && NombreCoincide(clip.name, take))
                    return clip;
            }
            if (clips.Length > 0 && clips[0] != null)
                return clips[0];
        }

        var go = ModelLoader.Load("Models/Mike/Mike@" + take, "Assets/Resources/Models/Mike/Mike@" + take + ".FBX");
        if (go == null)
            return null;

        var other = go.GetComponent<Animation>() ?? go.GetComponentInChildren<Animation>();
        if (other == null)
            return null;

        return BuscarClip(other, take) ?? PrimerClip(other);
    }

    static AnimationClip BuscarClip(Animation anim, string take)
    {
        if (anim == null)
            return null;

        var exact = anim.GetClip(take);
        if (exact != null)
            return exact;

        foreach (AnimationState state in anim)
        {
            if (state.clip != null && NombreCoincide(state.clip.name, take))
                return state.clip;
        }
        return null;
    }

    static AnimationClip PrimerClip(Animation anim)
    {
        if (anim == null)
            return null;
        foreach (AnimationState state in anim)
        {
            if (state.clip != null)
                return state.clip;
        }
        return null;
    }

    static bool NombreCoincide(string clipName, string take)
    {
        if (string.IsNullOrEmpty(clipName) || string.IsNullOrEmpty(take))
            return false;
        if (string.Equals(clipName, take, StringComparison.OrdinalIgnoreCase))
            return true;
        return clipName.IndexOf(take, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}

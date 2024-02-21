using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using v2;

public class CubeDemoMovement : MonoBehaviour
{
	private Transform4D t4d;

	// Start is called before the first frame update
	void Start()
    {
        t4d=GetComponent<Transform4D>();
        StartCoroutine(run());
    }

    IEnumerator run()
    {
        yield return new WaitForSeconds(1);

        for (float prog = 0; prog <= 1; prog += 1 / 360f)
        {
            float rad = Mathf.Lerp(0, 2*Mathf.PI, prog);
            //xy->xw
            //xz->yw
            t4d.localRotation[(int)Rot4D.xw] = t4d.localRotation[(int)Rot4D.yw] = rad;
            yield return new WaitForFixedUpdate();
        }
    }

}

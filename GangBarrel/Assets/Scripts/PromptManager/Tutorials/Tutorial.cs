using UnityEngine;
using UnityEngine.UI;


[CreateAssetMenu(fileName = "New Tutorial", menuName = "Tutorial")]
public class Tutorial : ScriptableObject
{
    public string header;
    public string description;
    // Use up to three smaller images 3x(9:16) or one bigger image 1x(16:9)
    public Sprite explanationImage1;
    public Sprite explanationImage2; // leave empty, if using only one
    public Sprite explanationImage3; // leave empty, if using only one
}

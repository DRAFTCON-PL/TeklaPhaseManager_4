using System;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;

public class TeklaRedrawView
{
    public static void Run(Tekla.Structures.Model.Operations.Operation op)
    {
        try
        {
            ViewHandler.SetRepresentation("+TPM_kolory");
            ModelViewEnumerator views = ViewHandler.GetAllViews();
            while (views.MoveNext())
            {
                View view = views.Current;
                view.ViewFilter = "+TPM_widocznosc";
                view.Modify();
                ViewHandler.RedrawView(view);
            }
        }
        catch { }
    }
}

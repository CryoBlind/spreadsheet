// Skeleton implementation by: Joe Zachary, Daniel Kopta, Travis Martin for CS 3500
// Last updated: August 2023 (small tweak to API)

using System.Collections;

namespace SpreadsheetUtilities;

/// <summary>
/// (s1,t1) is an ordered pair of strings 
/// t1 depends on s1; s1 must be evaluated before t1
/// 
/// A DependencyGraph can be modeled as a set of ordered pairs of strings.  Two ordered pairs
/// (s1,t1) and (s2,t2) are considered equal if and only if s1 equals s2 and t1 equals t2.
/// Recall that sets never contain duplicates.  If an attempt is made to add an element to a 
/// set, and the element is already in the set, the set remains unchanged.
/// 
/// Given a DependencyGraph DG:
/// 
///    (1) If s is a string, the set of all strings t such that (s,t) is in DG is called dependents(s).
///        (The set of things that depend on s)    
///        
///    (2) If s is a string, the set of all strings t such that (t,s) is in DG is called dependees(s).
///        (The set of things that s depends on) 
//
// For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
//     dependents("a") = {"b", "c"}
//     dependents("b") = {"d"}
//     dependents("c") = {}
//     dependents("d") = {"d"}
//     dependees("a") = {}
//     dependees("b") = {"a"}
//     dependees("c") = {"a"}
//     dependees("d") = {"b", "d"}
/// </summary>
public class DependencyGraph
{
    private int p_NumDependencies;
    private Hashtable? p_cells;

    public Hashtable Cells
    {
        get
        {
            return p_cells!;
        }
        private set
        {
            p_cells = value;
        }
    }
    /// <summary>
    /// Creates an empty DependencyGraph.
    /// </summary>
    public DependencyGraph()
    {
        Cells = new Hashtable();
        p_NumDependencies = 0;
    }


    /// <summary>
    /// The number of ordered pairs in the DependencyGraph.
    /// This is an example of a property.
    /// </summary>
    public int NumDependencies
    {
        get {return p_NumDependencies;}
    }


    /// <summary>
    /// Returns the size of dependees(s),
    /// that is, the number of things that s depends on.
    /// </summary>
    public int NumDependees(string s)
    {
        if ((Node)Cells[s]! == null) return 0;
        else return ((Node)Cells[s]!).Dependees.Count;
    }


    /// <summary>
    /// Reports whether dependents(s) is non-empty.
    /// </summary>
    public bool HasDependents(string s)
    {
        if((Node)Cells[s]! != null) return ((Node)Cells[s]!).Dependents.Count > 0;
        else return false;
    }


    /// <summary>
    /// Reports whether dependees(s) is non-empty.
    /// </summary>
    public bool HasDependees(string s)
    {
        if ((Node)Cells[s]! != null) return ((Node)Cells[s]!).Dependees.Count > 0;
        else return false;
    }


    /// <summary>
    /// Enumerates dependents(s).
    /// </summary>
    public IEnumerable<string> GetDependents(string s)
    {
        if ((Node)Cells[s]! != null) return ((Node)Cells[s]!).Dependents;
        else return Enumerable.Empty<string>();
    }


    /// <summary>
    /// Enumerates dependees(s).
    /// </summary>
    public IEnumerable<string> GetDependees(string s)
    {
        if ((Node)Cells[s]! != null) return ((Node)Cells[s]!).Dependees;
        else return Enumerable.Empty<string>();
    }


    /// <summary>
    /// <para>Adds the ordered pair (s,t), if it doesn't exist</para>
    /// 
    /// <para>This should be thought of as:</para>   
    /// 
    ///   t depends on s
    ///
    /// </summary>
    /// <param name="s"> s must be evaluated first. T depends on S</param>
    /// <param name="t"> t cannot be evaluated until s is</param>
    public void AddDependency(string s, string t)
    {
        bool shouldAddDependency = true;

        if (!Cells.ContainsKey(s))
        {
            //create key
            Cells.Add(s, new Node());
        }
        if (!Cells.ContainsKey(t))
        {
            //create key
            Cells.Add(t, new Node());
        }

        //add dependent and dependee relationship
        if (!((Node)Cells[t]!).Dependees.Contains(s))
        {
            ((Node)Cells[t]!).AddDependee(s);
            ((Node)Cells[s]!).AddDependent(t);
        }
        else shouldAddDependency = false;

        if (shouldAddDependency) p_NumDependencies++;
    }


    /// <summary>
    /// Removes the ordered pair (s,t), if it exists
    /// </summary>
    /// <param name="s"></param>
    /// <param name="t"></param>
    public void RemoveDependency(string s, string t)
    {
        bool shouldRemoveDepency = true;

        if (Cells[t] != null)
        {
            if (((Node)Cells[t]!).Dependees.Contains(s))
            {
                ((Node)Cells[t]!).RemoveDependee(s);
                ((Node)Cells[s]!).RemoveDependent(t);
            }
            else shouldRemoveDepency = false;
        }
        else shouldRemoveDepency = false;

        if (shouldRemoveDepency) p_NumDependencies--;
    }


    /// <summary>
    /// Removes all existing ordered pairs of the form (s,r).  Then, for each
    /// t in newDependents, adds the ordered pair (s,t).
    /// </summary>
    public void ReplaceDependents(string s, IEnumerable<string> newDependents)
    {
        if (!Cells.ContainsKey(s))
        {
            //create key
            Cells.Add(s, new Node());
        }

        var removed = ((Node)Cells[s]!).RemoveAllDependents();

        //remove dependee relationship from dependents
        foreach(String d in removed)
        {
            ((Node)Cells[d]!).RemoveDependee(s);
        }

        p_NumDependencies -= removed.Count;
        
        //adds new relationships
        foreach(String d in newDependents)
        {
            AddDependency(s, d);
        }
    }


    /// <summary>
    /// Removes all existing ordered pairs of the form (r,s).  Then, for each 
    /// t in newDependees, adds the ordered pair (t,s).
    /// </summary>
    public void ReplaceDependees(string s, IEnumerable<string> newDependees)
    {
        if (!Cells.ContainsKey(s))
        {
            //create key
            Cells.Add(s, new Node());
        }

        var removed = ((Node)Cells[s]!).RemoveAllDependees();
        //p_NumDependencies -= removed.Count;

        //remove dependent relationship from dependees
        foreach (String d in removed)
        {
            ((Node)Cells[d]!).RemoveDependent(s);
        }

        p_NumDependencies -= removed.Count;

        //adds new relationships
        foreach (String d in newDependees)
        {
            AddDependency(d, s);
        }
    }
}


class Node
{
    private List<String>? p_dependents;
    private List<String>? p_dependees;

    public List<String> Dependents
    {
        get{
            return p_dependents!;
        }
        private set{
            p_dependents = value;
        }
    }

    public List<String> Dependees
    {
        get
        {
            return p_dependees!;
        }
        private set
        {
            p_dependees = value;
        }
    }

    public Node()
    {
        Dependees = new List<String>();
        Dependents = new List<String>();
    }

    /// <summary>
    /// adds dependee
    /// </summary>
    /// <param name="dependee"></param>
    public void AddDependee(String dependee)
    {
        Dependees.Add(dependee);
    }

    /// <summary>
    /// adds dependent
    /// </summary>
    /// <param name="dependent"></param>
    public void AddDependent(String dependent)
    {
        Dependents.Add(dependent);
    }

    /// <summary>
    /// removes dependee
    /// </summary>
    /// <param name="dependee"></param>
    public void RemoveDependee(String dependee)
    {
        Dependees.Remove(dependee);
    }

    /// <summary>
    /// removes dependent
    /// </summary>
    /// <param name="dependent"></param>
    public void RemoveDependent(String dependent)
    {
        Dependents.Remove(dependent);
    }

    /// <summary>
    /// removes all dependents and returns a list of the previous dependents
    /// </summary>
    /// <returns>previous dependents</returns>
    public List<String> RemoveAllDependents()
    {
        var toReturn = Dependents;
        Dependents = new List<String>();
        return toReturn;
    }

    /// <summary>
    /// removes all dependees and returns a list of the previous dependees
    /// </summary>
    /// <returns>previous dependees</returns>
    public List<String> RemoveAllDependees()
    {
        var toReturn = Dependees;
        Dependees = new List<String>();
        return toReturn;
    }
}

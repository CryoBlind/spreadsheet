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
    private Hashtable p_cells;

    public Hashtable cells
    {
        get
        {
            return p_cells;
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
        cells = new Hashtable();
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
        return ((Node) cells[s]).dependees.Count;
    }


    /// <summary>
    /// Reports whether dependents(s) is non-empty.
    /// </summary>
    public bool HasDependents(string s)
    {
        return ((Node)cells[s]).dependents.Count > 0;
    }


    /// <summary>
    /// Reports whether dependees(s) is non-empty.
    /// </summary>
    public bool HasDependees(string s)
    {
        return ((Node)cells[s]).dependees.Count > 0;
    }


    /// <summary>
    /// Enumerates dependents(s).
    /// </summary>
    public IEnumerable<string> GetDependents(string s)
    {
        return ((Node) cells[s]).dependents;
    }


    /// <summary>
    /// Enumerates dependees(s).
    /// </summary>
    public IEnumerable<string> GetDependees(string s)
    {
        return ((Node)cells[s]).dependees;
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

        if (!cells.ContainsKey(s))
        {
            //create key
            cells.Add(s, new Node());
        }
        if (!cells.ContainsKey(t))
        {
            //create key
            cells.Add(t, new Node());
        }

        //add dependent and dependee relationship
        if (!((Node)cells[t]).dependees.Contains(s))
        {
            ((Node)cells[t]).addDependee(s);
        }
        else shouldAddDependency = false;

        if (!((Node)cells[s]).dependents.Contains(t))
        {
            ((Node)cells[s]).addDependent(t);
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

        if (cells[t] != null)
        {
            if (((Node)cells[t]).dependees.Contains(s)) ((Node)cells[t]).removeDependee(s);
            else shouldRemoveDepency = false;
        }
        else shouldRemoveDepency = false;

        if (cells[s] != null)
        {
            if (((Node)cells[s]).dependents.Contains(t)) ((Node)cells[s]).removeDependent(t);
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
        var removed = ((Node)cells[s]).removeAllDependents();

        //remove dependee relationship from dependents
        foreach(String d in removed)
        {
            ((Node)cells[d]).removeDependee(s);
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
        var removed = ((Node)cells[s]).removeAllDependees();

        //remove dependent relationship from dependees
        foreach (String d in removed)
        {
            ((Node)cells[d]).removeDependent(s);
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
    private List<String> p_dependents;
    private List<String> p_dependees;

    public List<String> dependents
    {
        get{
            return p_dependents;
        }
        private set{
            p_dependents = value;
        }
    }

    public List<String> dependees
    {
        get
        {
            return p_dependees;
        }
        private set
        {
            p_dependees = value;
        }
    }

    public Node()
    {
        dependees = new List<String>();
        dependents = new List<String>();
    }

    /// <summary>
    /// adds dependee
    /// </summary>
    /// <param name="dependee"></param>
    public void addDependee(String dependee)
    {
        dependees.Add(dependee);
    }

    /// <summary>
    /// adds dependent
    /// </summary>
    /// <param name="dependent"></param>
    public void addDependent(String dependent)
    {
        dependents.Add(dependent);
    }

    /// <summary>
    /// removes dependee
    /// </summary>
    /// <param name="dependee"></param>
    public void removeDependee(String dependee)
    {
        dependees.Remove(dependee);
    }

    /// <summary>
    /// removes dependent
    /// </summary>
    /// <param name="dependent"></param>
    public void removeDependent(String dependent)
    {
        dependents.Remove(dependent);
    }

    /// <summary>
    /// removes all dependents and returns a list of the previous dependents
    /// </summary>
    /// <returns>previous dependents</returns>
    public List<String> removeAllDependents()
    {
        var toReturn = dependents;
        dependents = new List<String>();
        return toReturn;
    }

    /// <summary>
    /// removes all dependees and returns a list of the previous dependees
    /// </summary>
    /// <returns>previous dependees</returns>
    public List<String> removeAllDependees()
    {
        var toReturn = dependees;
        dependees = new List<String>();
        return toReturn;
    }
}

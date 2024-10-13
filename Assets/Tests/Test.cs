using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class Test
{
    private Board board;

    [SetUp]
    public void Setup()
    {
        this.board = new Board();
        board.Won += pn => Debug.Log($"Player{pn+1} Win!");
    }

    [Test]
    public void Test1()
    {
        var locs = board.GetListOfAccessibleLocs(0);
        var elms = new HashSet<(int, int)>() { (8, 3), (8, 5), (7, 4) };
        Assert.That(locs, Is.SubsetOf(elms));
        Assert.False(board.TryPutWall(0, 8, 3, true));
        Assert.True(board.TryPutWall(0, 7, 3, true));
        Assert.True(board.TryPutWall(0, 7, 4, true));
        locs = board.GetListOfAccessibleLocs(0);
        foreach (var loc in locs)
        {
            Assert.AreEqual(loc, (7, 4));
        }
        board.Move(0, 7, 4);
        board.Move(1, 6, 4);
        locs = board.GetListOfAccessibleLocs(0);
        elms = new HashSet<(int, int)>() { (5, 4), (8, 4) };
        Assert.That(locs, Is.SubsetOf(elms));
        Assert.True(board.TryPutWall(0, 5, 4, false));
        locs = board.GetListOfAccessibleLocs(0);
        elms = new HashSet<(int, int)>() { (6, 3), (6, 5), (8, 4) };
        Assert.That(locs, Is.SubsetOf(elms));
    }
}

using System.Collections.Generic;
using NUnit.Framework;

public class Test
{
    private MatchBoardService _matchBoardService;

    [SetUp]
    public void Setup()
    {
        _matchBoardService = MatchBoardService.GetBoard();
    }

    [Test]
    public void Test1()
    {
        var locs = _matchBoardService.GetListOfAccessibleLocs(0);
        var elms = new HashSet<(int, int)> { (8, 3), (8, 5), (7, 4) };
        Assert.That(locs, Is.SubsetOf(elms));
        Assert.False(_matchBoardService.TryPutWall(0, 8, 3, true));
        Assert.True(_matchBoardService.TryPutWall(0, 7, 3, true));
        Assert.True(_matchBoardService.TryPutWall(0, 7, 4, true));
        locs = _matchBoardService.GetListOfAccessibleLocs(0);
        foreach (var loc in locs) Assert.AreEqual(loc, (7, 4));
        _matchBoardService.Move(0, 7, 4);
        _matchBoardService.Move(1, 6, 4);
        locs = _matchBoardService.GetListOfAccessibleLocs(0);
        elms = new HashSet<(int, int)> { (5, 4), (8, 4) };
        Assert.That(locs, Is.SubsetOf(elms));
        Assert.True(_matchBoardService.TryPutWall(0, 5, 4, false));
        locs = _matchBoardService.GetListOfAccessibleLocs(0);
        elms = new HashSet<(int, int)> { (6, 3), (6, 5), (8, 4) };
        Assert.That(locs, Is.SubsetOf(elms));
    }
}
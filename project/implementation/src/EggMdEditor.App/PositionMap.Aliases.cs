using System;

namespace EggMdEditor.App;

public class CorrespondenceEngine : PositionMap
{
    public CorrespondenceEngine() : base() { }
    public CorrespondenceEngine(object? ast) : base(ast) { }
}

public class PositionMapper : PositionMap
{
    public PositionMapper() : base() { }
    public PositionMapper(object? ast) : base(ast) { }
}

public class PositionMapping : PositionMap
{
    public PositionMapping() : base() { }
    public PositionMapping(object? ast) : base(ast) { }
}

public class PositionCorrespondence : PositionMap
{
    public PositionCorrespondence() : base() { }
    public PositionCorrespondence(object? ast) : base(ast) { }
}

public class CorrespondenceMap : PositionMap
{
    public CorrespondenceMap() : base() { }
    public CorrespondenceMap(object? ast) : base(ast) { }
}

public class AnchorMap : PositionMap
{
    public AnchorMap() : base() { }
    public AnchorMap(object? ast) : base(ast) { }
}

public class PositionMappingEngine : PositionMap
{
    public PositionMappingEngine() : base() { }
    public PositionMappingEngine(object? ast) : base(ast) { }
}

public class Correspondence : PositionMap
{
    public Correspondence() : base() { }
    public Correspondence(object? ast) : base(ast) { }
}

public class AnchorEngine : PositionMap
{
    public AnchorEngine() : base() { }
    public AnchorEngine(object? ast) : base(ast) { }
}

public class PositionCorrespondenceEngine : PositionMap
{
    public PositionCorrespondenceEngine() : base() { }
    public PositionCorrespondenceEngine(object? ast) : base(ast) { }
}

public class Anchor : PositionAnchor { }
public class RangePair : PositionAnchor { }
public class AnchorPair : PositionAnchor { }
public class CorrespondenceAnchor : PositionAnchor { }
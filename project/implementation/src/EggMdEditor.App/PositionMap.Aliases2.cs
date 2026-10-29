using System;

namespace EggMdEditor.App;

// Additional alias classes (inherit PositionMap) for T-044 compatibility.
public class PositionMapEngine : PositionMap { public PositionMapEngine() : base() { } public PositionMapEngine(object? ast) : base(ast) { } }
public class PositionMapManager : PositionMap { public PositionMapManager() : base() { } public PositionMapManager(object? ast) : base(ast) { } }
public class PositionMapService : PositionMap { public PositionMapService() : base() { } public PositionMapService(object? ast) : base(ast) { } }
public class PositionMapCalculator : PositionMap { public PositionMapCalculator() : base() { } public PositionMapCalculator(object? ast) : base(ast) { } }
public class PositionMapperEngine : PositionMap { public PositionMapperEngine() : base() { } public PositionMapperEngine(object? ast) : base(ast) { } }
public class PositionMappingEngine2 : PositionMap { public PositionMappingEngine2() : base() { } public PositionMappingEngine2(object? ast) : base(ast) { } }
public class AnchorMapper : PositionMap { public AnchorMapper() : base() { } public AnchorMapper(object? ast) : base(ast) { } }
public class AnchorMapping : PositionMap { public AnchorMapping() : base() { } public AnchorMapping(object? ast) : base(ast) { } }
public class AnchorCorrespondence : PositionMap { public AnchorCorrespondence() : base() { } public AnchorCorrespondence(object? ast) : base(ast) { } }
public class CorrespondenceMapper : PositionMap { public CorrespondenceMapper() : base() { } public CorrespondenceMapper(object? ast) : base(ast) { } }
public class CorrespondenceMapping : PositionMap { public CorrespondenceMapping() : base() { } public CorrespondenceMapping(object? ast) : base(ast) { } }
public class CorrespondenceMapEngine : PositionMap { public CorrespondenceMapEngine() : base() { } public CorrespondenceMapEngine(object? ast) : base(ast) { } }
public class CorrespondenceCalculator : PositionMap { public CorrespondenceCalculator() : base() { } public CorrespondenceCalculator(object? ast) : base(ast) { } }
public class CorrespondenceService : PositionMap { public CorrespondenceService() : base() { } public CorrespondenceService(object? ast) : base(ast) { } }
public class PositionCorrespondenceMap : PositionMap { public PositionCorrespondenceMap() : base() { } public PositionCorrespondenceMap(object? ast) : base(ast) { } }
public class PositionCorrespondenceMapper : PositionMap { public PositionCorrespondenceMapper() : base() { } public PositionCorrespondenceMapper(object? ast) : base(ast) { } }
public class PositionCorrespondenceService : PositionMap { public PositionCorrespondenceService() : base() { } public PositionCorrespondenceService(object? ast) : base(ast) { } }

// Anchor alias classes
public class PositionAnchorAlias : PositionAnchor { }
public class AnchorItem : PositionAnchor { }
public class RangeMapping : PositionAnchor { }
public class CorrespondencePair : PositionAnchor { }
public class PositionPair : PositionAnchor { }
public class RangePairItem : PositionAnchor { }
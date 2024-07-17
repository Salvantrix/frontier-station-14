using Content.Shared._NF.SectorServices.Prototypes;
using Content.Server.GameTicking;
using JetBrains.Annotations;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Content.Server.Administration.Logs.Converters;

namespace Content.Server._NF.SectorServices;

/// <summary>
/// System that manages sector-wide services.
/// Allows service components to be registered and unregistered on a singular entity
/// </summary>
[PublicAPI]
public sealed class SectorServiceSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    [ViewVariables(VVAccess.ReadOnly)]
    private EntityUid _entity = EntityUid.Invalid; // The station entity that's storing our services.

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationSectorServiceHostComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<StationSectorServiceHostComponent, ComponentShutdown>(OnComponentShutdown);
    }

    private void OnComponentStartup(EntityUid uid, StationSectorServiceHostComponent component, ComponentStartup args)
    {
            Log.Error($"OnComponentStartup! Entity: {uid} internal: {_entity}");
        if (_entity == EntityUid.Invalid)
        {
            _entity = uid;

            foreach (var servicePrototype in _prototypeManager.EnumeratePrototypes<SectorServicePrototype>())
            {
                Log.Error($"Adding component: {servicePrototype.Components}");
                _entityManager.AddComponents(_entity, servicePrototype.Components, false); // removeExisting false - do not override existing components.
            }
        }
    }

    private void OnComponentShutdown(EntityUid uid, StationSectorServiceHostComponent component, ComponentShutdown args)
    {
        Log.Error($"OnComponentShutdown! Entity: {_entity}");
        if (_entity != EntityUid.Invalid)
        {
            foreach (var servicePrototype in _prototypeManager.EnumeratePrototypes<SectorServicePrototype>())
            {
                Log.Error($"Removing component: {servicePrototype.Components}");
                _entityManager.RemoveComponents(_entity, servicePrototype.Components, false); // removeExisting false - do not override existing components.
            }
            _entity = EntityUid.Invalid;
        }
    }

    // Component access (mirroring EntityManager without entity ID)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetComponent<T>([NotNullWhen(true)] out T? component) where T : IComponent?
    {
        return _entityManager.TryGetComponent(_entity, typeof(T), component);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetComponent(Type type, [NotNullWhen(true)] out IComponent? component)
    {
        return _entityManager.TryGetComponent(_entity, type, component);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetComponent(CompIdx type, [NotNullWhen(true)] out IComponent? component)
    {
        return _entityManager.TryGetComponent(_entity, type, component);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetComponent([NotNullWhen(true)] EntityUid? uid, Type type,
        [NotNullWhen(true)] out IComponent? component)
    {
        return _entityManager.TryGetComponent(_entity, netId, component, meta);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetComponent(ushort netId, [MaybeNullWhen(false)] out IComponent component, MetaDataComponent? meta = null)
    {
        return _entityManager.TryGetComponent(_entity, netId, component, meta);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Pure]
    public bool TryComp([NotNullWhen(true)] out TComp1? component)
        => TryGetComponent(out component);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Pure]
    public bool TryComp([NotNullWhen(true)] out TComp1? component)
        => TryGetComponent(out component);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Pure]
    public TComp1? CompOrNull()
    {
        if (TryGetComponent(_entity, out var comp))
            return comp;
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Pure]
    public TComp1 Comp()
    {
        return GetComponent(_entity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetComponent<T>() where T : IComponent
    {
        return _entityManager.GetComponent<T>(_entity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IComponent GetComponent(CompIdx type)
    {
        return _entityManager.GetComponent(_entity, type);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IComponent GetComponent(Type type)
    {
        return _entityManager.GetComponent(_entity, type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Pure]
    public bool HasComp() => _entityManager.HasComponent(_entity);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Pure]
    public bool HasComp(EntityUid? uid) => HasComponent(uid);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Pure]
    public bool HasComponent(EntityUid uid)
    {
        return _traitDict.TryGetValue(uid, out var comp) && !comp.Deleted;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Pure]
    public bool HasComponent(EntityUid? uid)
    {
        return uid != null && HasComponent(uid.Value);
    }
}
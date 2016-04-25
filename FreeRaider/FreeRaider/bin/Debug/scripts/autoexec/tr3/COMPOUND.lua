function autoexec_PreLoad()
    static_tbl[10] = {coll = COLLISION_TYPE_STATIC, shape = COLLISION_SHAPE_TRIMESH};        -- Misaligned crate
    static_tbl[20] = {coll = COLLISION_TYPE_STATIC, shape = COLLISION_SHAPE_TRIMESH};        -- Truck
    static_tbl[21] = {coll = COLLISION_TYPE_STATIC, shape = COLLISION_SHAPE_BOX};            -- Truck wheels
    static_tbl[23] = {coll = COLLISION_TYPE_STATIC, shape = COLLISION_SHAPE_TRIMESH};        -- Dining desk 1
    static_tbl[24] = {coll = COLLISION_TYPE_STATIC, shape = COLLISION_SHAPE_TRIMESH};        -- Dining desk 2
    static_tbl[25] = {coll = COLLISION_TYPE_NONE,   shape = COLLISION_SHAPE_BOX};            -- Twin chain
    static_tbl[26] = {coll = COLLISION_TYPE_NONE,   shape = COLLISION_SHAPE_BOX};            -- Ceiling lamp
    static_tbl[30] = {coll = COLLISION_TYPE_STATIC, shape = COLLISION_SHAPE_TRIMESH};        -- Sink
    static_tbl[31] = {coll = COLLISION_TYPE_STATIC, shape = COLLISION_SHAPE_TRIMESH};        -- WC
    static_tbl[32] = {coll = COLLISION_TYPE_STATIC, shape = COLLISION_SHAPE_BOX};            -- Vertical pole
    static_tbl[33] = {coll = COLLISION_TYPE_STATIC, shape = COLLISION_SHAPE_BOX};            -- Grated panel
    static_tbl[34] = {coll = COLLISION_TYPE_STATIC, shape = COLLISION_SHAPE_TRIMESH};        -- Chair
    static_tbl[35] = {coll = COLLISION_TYPE_STATIC, shape = COLLISION_SHAPE_BOX};            -- Mainframe 1
    static_tbl[36] = {coll = COLLISION_TYPE_STATIC, shape = COLLISION_SHAPE_BOX};            -- Mainframe 2
    static_tbl[37] = {coll = COLLISION_TYPE_STATIC, shape = COLLISION_SHAPE_TRIMESH};        -- Sink set
    static_tbl[38] = {coll = COLLISION_TYPE_STATIC, shape = COLLISION_SHAPE_TRIMESH};        -- WC set
    static_tbl[39] = {coll = COLLISION_TYPE_NONE,   shape = COLLISION_SHAPE_BOX};            -- Keypad
    static_tbl[40] = {coll = COLLISION_TYPE_STATIC, shape = COLLISION_SHAPE_BOX};            -- Pipe
    static_tbl[41] = {coll = COLLISION_TYPE_NONE,   shape = COLLISION_SHAPE_BOX};            -- Food 1
    static_tbl[42] = {coll = COLLISION_TYPE_NONE,   shape = COLLISION_SHAPE_BOX};            -- Food 2
    static_tbl[47] = {coll = COLLISION_TYPE_NONE,   shape = COLLISION_SHAPE_BOX};            -- F-117
end;

function autoexec_PostLoad()
    addCharacterHair(player, HAIR_TR3);
    playStream(27);
end;
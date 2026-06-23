# TASKS.md

添加几个内置ParameterHandler，大部分都是可以基于TupleHandler和CompositeHandler实现的 ✅ **已完成**

关于内置Handler，你可以参考例如Vector2Handler、Vector3Handler这些

清单：
- [x] RectParameterHandler
  - 支持： Rect(float x, float y, float width, float height);
  - 支持： Rect(Vector2 position, Vector2 size);
- [x] RectIntParameterHandler
  - 支持： RectInt(int x, int y, int width, int height);
  - 支持： RectInt(Vector2Int position, Vector2Int size);
- [x] BoundsParameterHandler
  - 支持： Bounds(float x, float y, float z, float width, float height, float depth);
  - 支持： Bounds(Vector3 center, Vector3 size);
- [x] BoundsIntParameterHandler
  - 支持： BoundsInt(int x, int y, int z, int width, int height, int depth);
  - 支持： BoundsInt(Vector3Int position, Vector3Int size);
- [x] ColorParameterHandler
  - 支持： Color(float r, float g, float b, float a);
  - 支持： Color(float r, float g, float b); 认为a是1.0
  - 支持： Color(int r, int g, int b, int a);
  - 支持： Color(int r, int g, int b); 认为a是255
  - 支持： Color(string hex);
  - 支持： Color(string hex, float a); 认为a是1.0
- [x] GuidParameterHandler
  - 支持： Guid(string guid); （这个你就不能基于TupleHandler了）
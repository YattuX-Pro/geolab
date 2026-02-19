namespace Api.Dtos;

public record GpkgFileDto(
    Guid Id,
    string FileName,
    DateTime UploadDate,
    string? Description,
    int LayerCount,
    int TotalFeatureCount
);

public record GpkgFileDetailDto(
    Guid Id,
    string FileName,
    DateTime UploadDate,
    string? Description,
    List<GpkgLayerDto> Layers
);

public record GpkgLayerDto(
    Guid Id,
    string LayerName,
    string GeometryType,
    int FeatureCount
);

public record GpkgFeatureDto(
    Guid Id,
    string LayerName,
    object Geometry,
    Dictionary<string, object?> Properties
);

public record GpkgUploadResponseDto(
    Guid FileId,
    string FileName,
    int LayerCount,
    int TotalFeatureCount,
    string Message
);

public record GpkgFeaturesGeoJsonDto(
    string Type,
    List<GpkgGeoJsonFeatureDto> Features
);

public record GpkgGeoJsonFeatureDto(
    string Type,
    Guid Id,
    string LayerName,
    object Geometry,
    Dictionary<string, object?> Properties
);

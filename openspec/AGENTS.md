# OpenSpec — 本仓库 Agent 指引

本仓库使用 [OpenSpec](https://github.com/Fission-AI/OpenSpec) 风格的规格驱动开发：行为以 `openspec/specs/` 为事实来源；较大变更在 `openspec/changes/<change-id>/` 中保留提案、设计与任务拆解。

## 阅读顺序

1. `openspec/project.md` — 技术栈、构建命令、目录约定。
2. `openspec/specs/move-reminder/spec.md` — 当前产品行为与验收标准。
3. 若追溯某次功能来源：历史变更在 `openspec/changes/archive/`；较新增量见 `openspec/changes/2026-05-08-settings-modular-text-image/` 与 `openspec/project.md`「最近更新」表。

## 修改流程

- **改行为**：先更新 `specs/` 中对应 `spec.md` 的 ADDED/MODIFIED 段落，再改代码，最后同步 `changes/` 或归档说明。
- **新能力**：在 `changes/<id>/` 新建 `proposal.md`、`design.md`、`tasks.md` 与 spec delta，实现后再合并进 `specs/`。

## 构建与测试

见 `openspec/project.md` 中的命令。Agent 在提交前应至少完成一次 `dotnet build`（Release）。

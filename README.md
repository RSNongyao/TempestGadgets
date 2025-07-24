TempestGadgets - 一个[Hacknet](https://store.steampowered.com/app/365450/Hacknet/) [Pathfinder](https://github.com/Arkhist/Hacknet-Pathfinder)模组
===
TempestGadgets提供了一些新的exe, 允许玩家破解更多的端口

如果你遇到了Bug或者有什么改进的想法, 欢迎发issue或者在Discord **@rs_nongyao**

##  新的`Executibles`
每个exe都有重做的图像和新的机制:
- `#VPN_BYPASS#`|`VPNBypass.exe`:破解[0DTK](https://github.com/prodzpod/ZeroDayToolKit)的`123 (Network Time Protocol)`端口
  - 此exe需要`443 (HTTPS)`端口开启以运行
  - 在`123`端口开启后, `443`端口会被关闭, `VPNBypass`会保持运行直至被手动`kill`结束进程
  - 此exe被`kill`后, 会关闭已开启的`443`端口, 你需要在`VPNBypass`保持运行时开启`443`端口才能同时开启两个端口
- `#EOS_ROOTKIT#`|`eOSRootKit.exe`:破解`3659 (eOS Connection Manager)`端口
  - 此exe只能在管理员密码**不为**`alpine`的节点运行
  - 在`3659`端口开启后, 节点会在60s后自动重启, 并重设`3659`端口和**管理员密码以及管理员权限**---时刻注意节点状态!
- `#NET_SPOOF#`|`NetSpoof.exe`:破解`211 (Transfer)`端口
- `#FRWALL_DEFACER#`|`FirewallDefacer.exe`:破解防火墙(移植于[XMOD](https://github.com/tenesiss/Hacknet-Pathfinder-XMOD-Dev) (等待实施)
- `#SIGNAL_FILTER#`|`SignalFilter.exe`:破解`32 (SignalScramble)`端口 (等待实施)
- `#EN_BREAKER#`|`EnSecBreaker.exe`:通过EnSec直接获取节点管理员权限(移植于[XMOD](https://github.com/tenesiss/Hacknet-Pathfinder-XMOD-Dev) (等待实施)
- 等待更新

## 新的`Action`
- 等待更新

## 新的`Daemon`
- 等待更新

## 新的`Goals`
- 等待更新

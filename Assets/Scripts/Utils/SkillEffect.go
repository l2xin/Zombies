package main

import (
	"container/list"
	"base/glog"
	"time"
	"base/math"
	//"usercmd"
)

const (
	kEffectA 		= 150.0
	kEffectO 		= 200.0
	kEffectPerAg 	= 60.0 * 3.1415926 / 180.0 		// 每一帧转的弧度
	kEffectFoldX 	= 150.0  						// 折线每次走150.0拐弯
	kEffectRad 		= 100.0   						// 旋转半径
)

const (
	PATH_TYPE_DIRECT_LINE 		= 1 	// 直线
	PATH_TYPE_CIRCLE_ROTATION 	= 2 	// 圆形旋转
	PATH_TYPE_GO_BACK 			= 3 	// 来回
	PATH_TYPE_CURVE_LINE 		= 4 	// 正弦
	PATH_TYPE_FOLD_LINE 		= 5 	// 折线
)

type SkillEffect struct {
	playerId 			uint64
	playerType  		uint8
	skillid 			uint32 
	pathType 			uint32 			// 弹道类型
	count 				uint32 	 		// 运行帧数
	backMul 			float32 		// 回弹系数
	backAutoFlag 		bool

	castSeq  			uint32  		// 释放序列号
	arrivetime 			int64
	lastfoldtm 			int64  			// 上一次折走的时间(ms)
	flySpdVec 			math.Vector3 	// 飞行速度方向分量
	posVec 				math.Vector3 	// 技能抵达的位置
	prePosVec 			math.Vector3 	// 上一次抵达的位置
	oriPos 				math.Vector3 	// 起飞点
	flytime  			int64  			// 要飞行的时长(ms)
	flyspeed 			float32 		// 飞行速度
	skillRadius 		float32 		// 子弹半径
}

func NewSkillEffect(_plyId uint64, _plyType uint8, _arrivetime int64, _flytime int64, _skillid, _pathType uint32, _flyspeed float32, _castSeq uint32) *SkillEffect {
	return &SkillEffect{
		playerId: 		_plyId,
		playerType: 	_plyType,
		skillid: 		_skillid,
		pathType: 		_pathType,
		count: 			0,
		backMul: 		1.0,
		backAutoFlag: 	false,
		castSeq: 		_castSeq,
		arrivetime: 	_arrivetime,
		flytime: 		_flytime,
		flyspeed: 		_flyspeed,
		flySpdVec:		math.Vector3{},
		posVec: 		math.Vector3{},
		prePosVec: 		math.Vector3{},
		skillRadius: 	30.0,
	}
}

// 增加一个飞行轨迹
func (this *SkillEffect) addEffPosVec(owner *ScenePlayer,  nowmsec int64) {
	 this.prePosVec.CopyFrom( &this.posVec )
	 this.count = this.count + 1
	 switch this.pathType {
	 case PATH_TYPE_CIRCLE_ROTATION:
	 	// 围绕人物旋转
	 	this.posVec.X =  owner.Realpos.X + kEffectRad * math.CosF32( float32(this.count) * kEffectPerAg );
	 	this.posVec.Z =  owner.Realpos.Z + kEffectRad * math.SinF32( float32(this.count) * kEffectPerAg );
	 case PATH_TYPE_GO_BACK:
	 	// 原路返回
	 	this.posVec.X = this.posVec.X + this.flySpdVec.X * this.backMul
	 	this.posVec.Z = this.posVec.Z + this.flySpdVec.Z * this.backMul
	 case PATH_TYPE_CURVE_LINE:
	 	// 正弦曲线
	 	tpos := math.Vector3{X:0, Y:0, Z: 0} 	
	 	tpos.X = this.oriPos.X + float32(this.count) * (this.flyspeed * DEFAULT_ROOM_MSEC_PER_FRAME)/1000.0
	 	tpos.Z = this.oriPos.Z + kEffectA * math.SinF32(  kEffectO * (tpos.X - this.oriPos.X) )

	 	// 旋转坐标
	 	this.posVec.X = tpos.X * this.flySpdVec.X - tpos.Z * this.flySpdVec.Z
	 	this.posVec.Z = tpos.X * this.flySpdVec.Z + tpos.Z * this.flySpdVec.X
	 	glog.Info("[飞行轨迹] 正弦曲线=", this.posVec.X, " ", this.posVec.Z)

	 case PATH_TYPE_FOLD_LINE:
	 	if float32(nowmsec - this.lastfoldtm) * this.flyspeed / 1000.0 >= kEffectFoldX {
	 		this.lastfoldtm = nowmsec
	 		this.backMul = this.backMul * -1.0

	 		if this.backMul == 1.0 {
	 			this.flySpdVec.PerpUp()
	 		} else {
	 			this.flySpdVec.PerpDown()
	 		}
	 		glog.Info("[飞行轨迹] 折线飞行拐弯 this.posVec= ", this.posVec)
	 	}
	 	this.posVec.IncreaseBy( &this.flySpdVec )
	 	glog.Info("[飞行轨迹] 折线飞行 this.posVec= ", this.posVec)

	 case PATH_TYPE_DIRECT_LINE:
	 	this.posVec.IncreaseBy( &this.flySpdVec )
	 default:

	 }
}

// 检查是否返弹
func (this *SkillEffect) checkRebound(nowmsec int64) {
	if this.pathType == PATH_TYPE_GO_BACK && int64(this.count * DEFAULT_ROOM_MSEC_PER_FRAME) >= this.flytime / 2 && this.backAutoFlag == false {
		this.backMul = this.backMul * -1.0
		this.backAutoFlag = true
		glog.Info("飞行轨迹 达到一半时间开始返弹")
		return
	}
	// fixme 判断是否有阻档返弹
}

func (this *SkillEffect) setFlySpeedVec(owner *ScenePlayer, startPos, baseOrient *math.Vector3, nowmsec int64) {
	this.posVec.CopyFrom(startPos)
	
	this.flySpdVec.X = (this.flyspeed * baseOrient.X * DEFAULT_ROOM_MSEC_PER_FRAME / 1000.0)
	this.flySpdVec.Z = (this.flyspeed * baseOrient.Z * DEFAULT_ROOM_MSEC_PER_FRAME / 1000.0)

	if this.pathType == PATH_TYPE_GO_BACK {
		this.flytime = this.flytime * 2
	}

	if this.pathType == PATH_TYPE_FOLD_LINE {
		// 向左上角旋转45度
		x0, y0 := this.flySpdVec.X, this.flySpdVec.Z
		this.flySpdVec.X = x0 * 0.707 - y0 * 0.707
		this.flySpdVec.Z = x0 * 0.707 + y0 * 0.707

		if this.flyspeed != 0 {
			this.lastfoldtm = int64(float32(nowmsec - (kEffectFoldX * 500.0)) / this.flyspeed)
		}
	}

	glog.Info("[技能效果] addSkillEffect baseOrient= ", baseOrient, " flySpdVec= ", this.flySpdVec, " orientVec= ", owner.orientVec, " baseOrient= ", baseOrient, " 玩家位置= ", owner.Realpos, " posVec= ", this.posVec)

	//glog.Info("[技能飞行轨迹] ", msg, " count= ", kcount, " flytime= ", this.flytime)
}

func (this *SkillEffect) getOffsetSkillId() uint32 {
	return this.skillid << kSkillBit | this.castSeq
}



/**
 * 正在运行的技能管理
 */
type SkillEffectMgr struct {
	list_se *list.List
	scene *Scene
	frame int32
}

func NewSkillEffectMgr(scn *Scene) *SkillEffectMgr {
	return &SkillEffectMgr{ list_se: list.New(), scene: scn }
}


/** 增加技能效果 */
func (this *SkillEffectMgr) addSkillEffect( owner *ScenePlayer, skill *Skill, startPos, baseOrient *math.Vector3, castSeq uint32) *SkillEffect {
	var (
		nowmsec int64 = time.Now().UnixNano() / 1000000 
		arrivetime int64 = nowmsec // + skill.LeadTime
		fly_speed  float32 = owner.skillmgr.getFlySpeed()
		flytime    int64 = 0
		pathType   uint32 = owner.skillmgr.getSkillPathType()
	)
	if fly_speed != 0 {
		flytime = int64(owner.skillmgr.getFlyAtkDis() * 1000 / fly_speed)
	}

	ret := NewSkillEffect(owner.GetId(), owner.GetType(), arrivetime, flytime, skill.SkillId, pathType, fly_speed, castSeq)
	if nil == ret {
		return nil
	}
	ret.setFlySpeedVec(owner, startPos, baseOrient, nowmsec)

	this.list_se.PushBack(ret)
	return ret
}


func (this *SkillEffectMgr) timeAction() {
	nowmsec := time.Now().UnixNano()/1000000

	var (
		next 	*list.Element
	)

	for e := this.list_se.Front(); e != nil; {
		effect := e.Value.(*SkillEffect)
		if nil == effect {
			next = e.Next()
			this.list_se.Remove(e)
			e = next
			continue
		}

		if effect.arrivetime > nowmsec {
			e = e.Next()
			continue
		}

		var ret bool = this.onSkillArrived(effect, nowmsec)

		// 在飞行期间没有击中对象
		if 	ret == false {
			e = e.Next()
			continue
		}

		next = e.Next()
		this.list_se.Remove(e)
		e = next
		continue
	}
}

func (this *SkillEffectMgr) onSkillArrived(effect *SkillEffect, nowmsec int64) bool {
	var player *ScenePlayer = nil
	if effect.playerType == SCENE_TYPE_PLAYER {
		player, _ = this.scene.GetPlayer( effect.playerId )
	}
	if nil == player {
		return false
	}

	if skill, ok := player.skillmgr.skills[effect.skillid]; ok {
		effect.addEffPosVec(player, nowmsec)
		effect.checkRebound(nowmsec)
		return skill.OnHit(player, nowmsec, effect)
	}
	return false
}




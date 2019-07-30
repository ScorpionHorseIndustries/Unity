function OnUpdate_Chicken(chicken,deltaTime)
	local state = chicken.state

	if (state == nil) then
		state = "idle"
		

	end
	
	if (state == "reset") then
		state = "idle"
	elseif (state == "idle") then
		local wanderTimer = chicken.info:GetFloat("wanderTimer", math.random(1,5))
		if (wanderTimer <= 0) then
			wanderTimer = math.random(1,5)
			state = "wander"
			chicken.info:SetString("stateWhenMoved", "idle")
		else 
			wanderTimer = wanderTimer - deltaTime
			
		end
		chicken.info:SetFloat("wanderTimer", wanderTimer)
	elseif (state == "wander") then
		local target = World.current:FindNearByEmptyTile(chicken.pos)
		if (target ~= nil) then
			if (chicken:FindPath(target,false)) then
				state = "move"	
			
			end
		else
			state = "idle"
		end
	elseif (state == "move") then
		if (chicken.pos == chicken.dst) then

			dst = chicken.path:GetNextTile()
			if (dst == nil) then
				chicken.path = nil
				state = "finished_moving"
			else	
				chicken:SetDst(dst)

			end
		else
			chicken:Move(deltaTime)
			
		end		
	elseif (state == "find_path") then
		
	elseif (state == "finished_moving") then
		state = "reset"

	 end

	if (chicken.NewState ~= nil) then
		chicken.state = chicken.NewState
		chicken.NewState = nil
	else
		chicken.state = state
	end
end
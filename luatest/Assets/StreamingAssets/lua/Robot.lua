function OnUpdate_Robot(robot, deltaTime) 
	

	state = robot.info:GetString("state")
	if (state == nil) then
		state = "idle"
		

	end

	if (state == "idle") then
		state = "find_path"

	elseif (state == "find_path") then
		target = World.current:GetRandomEmptyTile()
		if (target ~= nil) then
			if (robot:FindPath(target,false)) then
				state = "move"	
			
			end
		end

	elseif (state == "move") then
		if (robot.pos == robot.dst) then

			dst = robot.path:GetNextTile()
			if (dst == nil) then
				robot.path = nil
				state = "idle"
			else	
				robot:SetDst(dst)

			end
		else
			robot:Move(deltaTime)
			
		end

	end

	robot.info:SetString("state", state);
	
	
	sayTimer = robot.info:GetFloat("sayTimer")
	if (sayTimer <= 0) then
		robot:Say(state)
		sayTimer = 2
	else 
		sayTimer = sayTimer - deltaTime
	end
	robot.info:SetFloat("sayTimer", sayTimer)


	
	--[[
	
	
	if (robot.pos == robot.dst) then
		t = robot.pos:GetRandomNeighbour()
		if (t ~= nil) then
			robot:SetDst(t)
			robot:Move(deltaTime)
		else
			
		end 
	else
		robot:Move(deltaTime)
	end
	]]

end

--[[

	if nearby job requires materials
		fetch materials

	if nearby stockpile is not full
		fill

	if nearby stockpile is completely empty
		fill

	if nearby tile needs work
		work tile

	fetch materials
		find materials
		foreach 
			allocate
			add to list
		
		foreach
			find path
			go
			pick up
			if full goto end


]]
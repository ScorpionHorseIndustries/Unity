function OnUpdate_Robot(robot, deltaTime) 
	

	local state = robot.state
	local saveState = state

	if (state == nil) then
		state = "idle"
		

	end

	if (state == "reset") then
		robot:DropAll()
		state = "idle"
		robot:ReturnWork()
		

	elseif (state == "idle") then
		local findWorkTimer = robot.info:GetFloat("findWorkTimer");
		if (findWorkTimer <= 0) then
			state = "find_work"
			findWorkTimer = math.random(1,2)
			robot.info:SetFloat("waitForWorkTimer", math.random(1,3))
		else	
			findWorkTimer = findWorkTimer - deltaTime
		end
		robot.info:SetFloat("findWorkTimer", findWorkTimer)

		local wanderTimer = robot.info:GetFloat("wanderTimer", math.random(1,5))
		if (wanderTimer <= 0) then
			wanderTimer = math.random(1,5)
			state = "wander"
			robot.info:SetString("stateWhenMoved", "idle")
		else 
			wanderTimer = wanderTimer - deltaTime
			
		end
		robot.info:SetFloat("wanderTimer", wanderTimer)

	elseif  (state == "find_work") then
		local waitForWorkTimer = robot.info:GetFloat("waitForWorkTimer")
		
		if (waitForWorkTimer <= 0) then
			state = "idle"
		else
			waitForWorkTimer = waitForWorkTimer - deltaTime
		end
		robot.info:SetFloat("waitForWorkTimer", waitForWorkTimer)
	--[[
		robot:GetWork()
		if (robot.work ~= nil) then
			state = "init_work"
		else	
			state = "idle"
		end
		]]
	elseif (state == "init_work") then
		robot.info:SetBool("findNeighbourTiles", true)
		if (robot.work ~= nil) then
			if (robot.work.inventoryItemName ~= nil) then
				state = "find_item"
			else
				--go to work tile
				state = "find_path_work.tile"
				robot.info:SetString("stateWhenMoved", "work")
				robot.info:SetBool("findNeighbourTiles", false)		
			end
		else
			state = "idle"
		end
	elseif (state == "wander") then
		local target = World.current:FindNearByEmptyTile(robot.pos)
		if (target ~= nil) then
			if (robot:FindPath(target,false)) then
				state = "move"	
			
			end
		else
			state = "idle"
		end
	elseif (state == "find_item") then
		--find some of that then
		local target = World.current.inventoryManager:GetNearest(robot.pos, robot.work.inventoryItemName,robot.work.inventorySearchStockpiles)
		--World.dbug("target:" + target.ToString() + "\nitem:" + work.inventoryItemName)
		if (target ~= nil) then
			robot.info:SetInt("goto_x", target.world_x);		
			robot.info:SetInt("goto_y", target.world_y);		
			state = "find_path_x_y"
			robot.info:SetString("stateWhenMoved", "pickup")
			robot.info:SetString("pathType", "xy");
			qtyToAllocate = robot.work.inventoryItemQtyRemaining - robot.inventory:HowMany(robot.work.inventoryItemName)
			target:InventoryAllocate(robot.work.inventoryItemName, qtyToAllocate)

		else
			state = "reset"
		end		
	elseif (state == "pickup") then
		local fx = robot.info:GetInt("goto_x")
		local fy = robot.info:GetInt("goto_y")
		local tile = World.current:GetTileAt(fx, fy)

		

		qtyToTake = robot.work.inventoryItemQtyRequired - robot.inventory:HowMany(robot.work.inventoryItemName)
		World.dbug("qty to pick up : " .. qtyToTake)

		if (robot:Pickup(tile, robot.work.inventoryItemName, qtyToTake)) then
			if (robot.inventory:HowMany(robot.work.inventoryItemName) >= robot.work.inventoryItemQtyRequired) then
				state = "find_path_work.tile"
				robot.info:SetString("stateWhenMoved", "work")
				robot.info:SetString("pathType", "tile");
			else
				state = "find_item"
			end
		else
			state = "find_item"
		end
	elseif (state == "find_new_path") then
		pathType = robot.info:GetString("pathType")

		if (pathType == "xy") then
			state = "find_path_x_y"
		else
			state = "find_path_work.tile"
		end
	elseif (state == "work") then
		robot.work:Work(deltaTime)

	elseif (state == "find_path_x_y") then
		fx = robot.info:GetInt("goto_x")
		fy = robot.info:GetInt("goto_y")

		if (robot:FindPath(fx, fy, true)) then
			state = "move"
		end
	elseif (state == "find_path_work.tile") then
		if (robot:FindPath(robot.work.workTile,robot.info:GetBool("findNeighbourTiles"))) then
			state = "move"
		else	
			state = "reset"
		end
	elseif (state == "move") then
		if (robot.pos == robot.dst) then

			dst = robot.path:GetNextTile()
			if (dst == nil) then
				robot.path = nil
				state = "finished_moving"
			else	
				robot:SetDst(dst)

			end
		else
			robot:Move(deltaTime)
			
		end
	elseif (state == "finished_moving") then
		stateWhenMoved = robot.info:GetString("stateWhenMoved")
		if (stateWhenMoved == nil or stateWhenMoved == "") then
			state = "idle"
		else		
			state = stateWhenMoved
		end
		robot.info:SetString("stateWhenMoved", "idle")
	end
	if (robot.NewState ~= nil) then
		robot.state = robot.NewState
		robot.NewState = nil
	else
		robot.state = state
	end
	
	
	
	
	sayTimer = robot.info:GetFloat("sayTimer")
	if (sayTimer <= 0) then
		if (saveState ~= state) then
			robot:Say(state)
		end
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
function OnUpdate_Robot(robot, deltaTime) 
	

	state = robot.state
	if (state == nil) then
		state = "idle"
		

	end

	if (state == "reset") then
		robot:DropAll()
		state = "idle"
		robot:ReturnWork()
		

	elseif (state == "idle") then
		findWorkTimer = robot.info:GetFloat("findWorkTimer");
		if (findWorkTimer <= 0) then
			state = "find_work"
			findWorkTimer = 1
			robot.info:SetFloat("waitForWorkTimer", 2)
		else	
			findWorkTimer = findWorkTimer - deltaTime
		end
		robot.info:SetFloat("findWorkTimer", findWorkTimer)

		wanderTimer = robot.info:GetFloat("wanderTimer", 5)
		if (wanderTimer <= 0) then
			wanderTimer = 5
			state = "wander"
			robot.info:SetString("stateWhenMoved", "idle")
		else 
			wanderTimer = wanderTimer - deltaTime
			
		end
		robot.info:SetFloat("wanderTimer", wanderTimer)

	elseif  (state == "find_work") then
		waitForWorkTimer = robot.info:GetFloat("waitForWorkTimer")
		
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
		if (robot.work ~= nil) then
			if (robot.work.inventoryItemName ~= nil) then
				state = "find_item"
			else
				--go to work tile
				state = "find_path_work.tile"
				robot.info:SetString("stateWhenMoved", "work")
			end
		else
			state = "idle"
		end
	elseif (state == "wander") then
		target = World.current:GetRandomEmptyTile()
		if (target ~= nil) then
			if (robot:FindPath(target,false)) then
				state = "move"	
			
			end
		else
			state = "idle"
		end
	elseif (state == "find_item") then
		--find some of that then
		target = World.current.inventoryManager:GetNearest(robot.pos, robot.work.inventoryItemName,robot.work.inventorySearchStockpiles)
		--World.dbug("target:" + target.ToString() + "\nitem:" + work.inventoryItemName)
		if (target ~= nil) then
			robot.info:SetInt("goto_x", target.world_x);		
			robot.info:SetInt("goto_y", target.world_y);		
			state = "find_path_x_y"
			robot.info:SetString("stateWhenMoved", "pickup")
			qtyToAllocate = robot.work.inventoryItemQtyRemaining - robot.inventory:HowMany(robot.work.inventoryItemName)
			target:InventoryAllocate(robot.work.inventoryItemName, qtyToAllocate)


		end		
	elseif (state == "pickup") then
		fx = robot.info:GetInt("goto_x")
		fy = robot.info:GetInt("goto_y")
		tile = World.current:GetTileAt(fx, fy)
		qtyToTake = robot.work.inventoryItemQtyRemaining - robot.inventory:HowMany(robot.work.inventoryItemName)


		if (robot:Pickup(tile, robot.work.inventoryItemName, qtyToTake)) then
			if (robot.inventory:HowMany(robot.work.inventoryItemName) >= robot.work.inventoryItemQtyRequired) then
				state = "find_path_work.tile"
				robot.info:SetString("stateWhenMoved", "work")
			else
				state = "find_item"
			end
		else
			state = "find_item"
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
		if (robot:FindPath(robot.work.workTile,true)) then
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

	robot.state = state
	
	
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
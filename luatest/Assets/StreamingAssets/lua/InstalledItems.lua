

function OnUpdate_Dummy(item,deltaTime) 


end

function Door_UpdateActions(item, deltaTime) 
	
end

function Workstation_UpdateActions(item, deltaTime) 
    tile = item:GetWorkSpot()


    if (tile.HasPendingJob == false) then
      j = Job.MakeStandardJob(
        tile,
        Workstation_JobComplete,
        Workstation_JobCancelled,
        JOB_TYPE.WORK_AT_STATION,
        1,
        item.workRecipeName

        )

      world.current.jobQueue:Push(j)
    end
end

function Stockpile_UpdateActions(item, deltaTime) 
	
end

namespace MinLib.Utility;

public class Git
{
    private string src;

    public Git(string src)
    {
        this.src = src;
    }

    public void NewBranch(string branch)
    {
        string? command = $"git checkout -b {branch}";
        Command.Execute(command, this.src);
    }

    public void Switch(string branch)
    {
        string? command = $"git checkout {branch}";
        Command.Execute(command, this.src);
    }

    public void Pull()
    {
        string? command = "git pull";
        Command.Execute(command, this.src);
    }

    public void Reset(int step = 0)
    {
        string? command = step == 0 ? "git reset --hard head"
                                    : $"git reset --hard head~{step}";
        Command.Execute(command, this.src);
    }

    public void ResetTo(string commitId)
    {
        string? command = $"git reset --hard {commitId}";
        Command.Execute(command, this.src);
    }

    public void ResetBranch(string branch)
    {
        Switch(branch);
        Reset();
    }

    public void ResetBranchTo(string branch, string commitId)
    {
        Switch(branch);
        Pull();
        ResetTo(commitId);
    }

    public void Merge(string branch)
    {
        string? command = $"git merge {branch}";
        Command.Execute(command, this.src);
    }

    public void MergeRemote(string branch, string remote = "origin")
    {
        string? command = $"git merge {remote}/{branch}";
        Command.Execute(command, this.src);
    }
}
